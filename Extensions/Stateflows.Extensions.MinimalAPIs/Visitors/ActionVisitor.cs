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

internal class ActionVisitor(RouteGroupBuilder behaviorsBuilder, System.Action<RouteHandlerBuilder> routeHandlerBuilderAction) : Actions.ActionVisitor, IBehaviorClassVisitor
{
    private readonly Dictionary<string, RouteGroupBuilder> RouteGroups = new();

    public RouteGroupBuilder GetRouteGroup(string behaviorClassName)
    {
        if (!RouteGroups.TryGetValue(behaviorClassName, out var action))
        {
            action = behaviorsBuilder.MapGroup($"/{behaviorClassName}");
            RouteGroups.Add(behaviorClassName, action);
            
            RegisterStandardEndpoints(behaviorClassName, action);
        }

        return action;
    }

    public Dictionary<string, List<HateoasLink>> CustomHateoasLinks { get; set; } = new();
    public void AddLink(HateoasLink link, string scope = "")
    {
        if (!CustomHateoasLinks.TryGetValue(scope, out var links))
        {
            links = new List<HateoasLink>();
            CustomHateoasLinks.Add(scope, links);
        }
        
        links.Add(link);
    }

    public override Task ActionAddedAsync(string actionName, int actionVersion)
    {
        GetRouteGroup(actionName);
        
        return Task.CompletedTask;
    }

    private static string GetEventName<TEvent>()
        => JsonNamingPolicy.CamelCase.ConvertName(Event<TEvent>.Name.ToShortName());

    private void RegisterStandardEndpoints(string actionName, RouteGroupBuilder action)
    {
        routeHandlerBuilderAction(
            action.MapGet("/", async (IStateflowsStorage storage) =>
            {
                IEnumerable<BehaviorClass> actionClasses = [new ActionClass(actionName)];
                var contexts = await storage.GetAllContextsAsync(actionClasses);
                return Results.Ok(contexts.Select(context => context.Id));
            })
        );
        
        routeHandlerBuilderAction(
            action.MapGet("/{instance}/status",
                async (string instance, IActionLocator locator) =>
                {
                    if (locator.TryLocateAction(new ActionId(actionName, instance), out var behavior))
                    {
                        var result = await behavior.GetStatusAsync([new NoImplicitInitialization()]);
                        return result.ToResult([], result.Response, CustomHateoasLinks);
                    }
                    
                    return Results.NotFound();
                }
            )
        );

        routeHandlerBuilderAction(
            action.MapPost("/{instance}/initialize",
                async (string instance, IActionLocator locator, RequestBody payload) =>
                {
                    if (locator.TryLocateAction(new ActionId(actionName, instance), out var behavior))
                    {
                        var result = await behavior.SendAsync(new Initialize());
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        var notifications = (await behavior.GetNotificationsAsync(payload.RequestedNotifications)).Response.Notifications.ToArray();
                        return result.ToResult(notifications, behaviorInfo, CustomHateoasLinks);
                    }
                    
                    return Results.NotFound();
                }
            )
        );

        routeHandlerBuilderAction(
            action.MapGet(
                "/{instance}/notifications",
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
                        var result = await behavior.GetNotificationsAsync(names, period);
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return result.ToResult(result.Response.Notifications, behaviorInfo, CustomHateoasLinks);
                    }
                    return Results.NotFound();
                })
        );


        routeHandlerBuilderAction(
            action.MapPost("/{instance}/finalize",
                async (string instance, IActionLocator locator, RequestBody payload) =>
                {
                    if (locator.TryLocateAction(new ActionId(actionName, instance), out var behavior))
                    {
                        var result = await behavior.FinalizeAsync();
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        var notifications = (await behavior.GetNotificationsAsync(payload.RequestedNotifications)).Response.Notifications.ToArray();
                        return result.ToResult(notifications, behaviorInfo, CustomHateoasLinks);
                    }
                    
                    return Results.NotFound();
                }
            )
        );

        routeHandlerBuilderAction(
            action.MapDelete("/{instance}", 
                async (string instance, IActionLocator locator) =>
                {
                    if (locator.TryLocateAction(new ActionId(actionName, instance), out var behavior))
                    {
                        var result = await behavior.ResetAsync();
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return result.ToResult([], behaviorInfo, CustomHateoasLinks);
                    }
                    
                    return Results.NotFound();
                }
            )
        );
    }

    public override Task ActionTypeAddedAsync<TAction>(string actionName, int actionVersion)
    {
        if (typeof(IActionEndpoints).IsAssignableFrom(typeof(TAction)))
        {
            var endpointsBuilder = new EndpointsBuilder(this, new ActionClass(actionName));
            
            var action = (IActionEndpoints)StateflowsActivator.CreateUninitializedInstance<TAction>();
            action.RegisterEndpoints(endpointsBuilder);
        }
        
        return Task.CompletedTask;
    }
}