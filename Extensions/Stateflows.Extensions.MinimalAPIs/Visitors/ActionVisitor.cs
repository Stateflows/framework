using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Stateflows.Common;
using Stateflows.Actions;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;

namespace Stateflows.Extensions.MinimalAPIs;

internal class ActionVisitor(IEndpointRouteBuilder routeBuilder, Interceptor interceptor)
    : Actions.ActionVisitor, IBehaviorClassVisitor
{
    public IEndpointRouteBuilder RouteBuilder => routeBuilder;
    public Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> HateoasLinks { get; set; } = new();

    public override Task ActionAddedAsync(string actionName, int actionVersion)
    {
        RegisterStandardEndpoints(actionName, routeBuilder);
        
        return Task.CompletedTask;
    }

    private static string GetEventName<TEvent>()
        => JsonNamingPolicy.CamelCase.ConvertName(Event<TEvent>.Name.ToShortName());

    private void RegisterStandardEndpoints(string actionName, IEndpointRouteBuilder action)
    {
        var behaviorClass = new ActionClass(actionName);
        
        var method = HttpMethods.Get;
        var route = $"/{actionName}";
        if (interceptor.BeforeGetInstancesEndpointDefinition(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = action.MapMethods(route, [method], async (IStateflowsStorage storage) =>
            {
                BehaviorClass[] actionClasses = [new ActionClass(actionName)];
                var contextIds = await storage.GetAllContextIdsAsync(actionClasses);
                return Results.Ok(contextIds.Select(id => new { Id = id }));
            });

            interceptor.AfterGetInstancesEndpointDefinition(behaviorClass, method, route, routeHandlerBuilder);
        }

        route = $"/{actionName}/{{instance}}/status";
        method = HttpMethods.Get;
        if (interceptor.BeforeEventEndpointDefinition<BehaviorInfoRequest>(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = action.MapMethods(
                route,
                [method],
                async (
                    string instance,
                    IActionLocator locator
                ) =>
                {
                    if (locator.TryLocateAction(new ActionId(actionName, instance), out var behavior))
                    {
                        var result = await behavior.GetStatusAsync([new NoImplicitInitialization()]);
                        // workaround for return code 200 regardless behavior actual status
                        result.Status = EventStatus.Consumed;
                        return result.ToResult([], result.Response, HateoasLinks);
                    }

                    return Results.NotFound();
                }
            );

            interceptor.AfterEventEndpointDefinition<BehaviorInfoRequest>(behaviorClass, method, route, routeHandlerBuilder);
            
            HateoasLinks.AddLink(
                behaviorClass.Name,
                new HateoasLink()
                {
                    Rel = "status",
                    Href = route,
                    Method = method
                },
                [BehaviorStatus.NotInitialized, BehaviorStatus.Initialized, BehaviorStatus.Finalized]
            );
        }

        route = $"/{actionName}/{{instance}}/notifications";
        method = HttpMethods.Get;
        if (interceptor.BeforeEventEndpointDefinition<NotificationsRequest>(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = action.MapMethods(
                route,
                [method],
                async (
                    IActionLocator locator,
                    string instance,
                    [FromQuery] string[] names,
                    [FromQuery] TimeSpan? period
                ) =>
                {
                    period ??= TimeSpan.FromSeconds(60);
                    if (locator.TryLocateAction(new ActionId(actionName, instance), out var behavior))
                    {
                        var result = await behavior.GetNotificationsAsync(names, period, [new NoImplicitInitialization()]);
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return result.ToResult(result.Response.Notifications, behaviorInfo, HateoasLinks);
                    }
                    return Results.NotFound();
                });
            
            interceptor.AfterEventEndpointDefinition<NotificationsRequest>(behaviorClass, method, route, routeHandlerBuilder);
            
            HateoasLinks.AddLink(
                behaviorClass.Name,
                new HateoasLink()
                {
                    Rel = "notifications",
                    Href = route,
                    Method = method
                },
                [BehaviorStatus.Initialized, BehaviorStatus.Finalized]
            );
        }

        route = $"/{actionName}/{{instance}}/finalize";
        method = HttpMethods.Post;
        if (interceptor.BeforeEventEndpointDefinition<Finalize>(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = action.MapMethods(
                route,
                [method],
                async (
                    string instance,
                    IActionLocator locator
                ) =>
                {
                    if (locator.TryLocateAction(new ActionId(actionName, instance), out var behavior))
                    {
                        var result = await behavior.FinalizeAsync();
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return result.ToResult([], behaviorInfo, HateoasLinks);
                    }
                    
                    return Results.NotFound();
                }
            );
            
            interceptor.AfterEventEndpointDefinition<Finalize>(behaviorClass, method, route, routeHandlerBuilder);
            
            HateoasLinks.AddLink(
                behaviorClass.Name,
                new HateoasLink()
                {
                    Rel = "finalize",
                    Href = route,
                    Method = method
                },
                [BehaviorStatus.Initialized]
            );
        }

        route = $"/{actionName}/{{instance}}";
        method = HttpMethods.Delete;
        if (interceptor.BeforeEventEndpointDefinition<Reset>(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = action.MapMethods(
                route,
                [method],
                async (
                    string instance,
                    IActionLocator locator
                ) =>
                {
                    if (locator.TryLocateAction(new ActionId(actionName, instance), out var behavior))
                    {
                        var result = await behavior.ResetAsync();
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return result.ToResult([], behaviorInfo, HateoasLinks);
                    }
                    
                    return Results.NotFound();
                }
            );
            
            interceptor.AfterEventEndpointDefinition<Reset>(behaviorClass, method, route, routeHandlerBuilder);
            
            HateoasLinks.AddLink(
                behaviorClass.Name,
                new HateoasLink()
                {
                    Rel = "reset",
                    Href = route,
                    Method = method
                },
                [BehaviorStatus.Initialized, BehaviorStatus.Finalized]
            );
        }
    }

    public override Task ActionTypeAddedAsync<TAction>(string actionName, int actionVersion)
    {
        if (typeof(IActionEndpoints).IsAssignableFrom(typeof(TAction)))
        {
            var endpointsBuilder = new EndpointsBuilder(routeBuilder, this, interceptor, new ActionClass(actionName));
            
            var action = (IActionEndpoints)StateflowsActivator.CreateUninitializedInstance<TAction>();
            action.RegisterEndpoints(endpointsBuilder);
        }
        
        return Task.CompletedTask;
    }
}