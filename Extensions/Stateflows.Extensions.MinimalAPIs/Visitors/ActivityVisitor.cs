using System.Text.Json;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Activities;
using Stateflows.Extensions.MinimalAPIs.Attributes;

namespace Stateflows.Extensions.MinimalAPIs;

internal class ActivityVisitor(
    IEndpointRouteBuilder routeBuilder,
    Interceptor interceptor,
    ITypeMapper typeMapper
) : Activities.ActivityVisitor, IBehaviorClassVisitor, ITypeVisitor
{
    public IEndpointRouteBuilder RouteBuilder => routeBuilder;
    public Interceptor Interceptor => interceptor;
    private readonly Dictionary<string, bool> Initializers = new();
    private readonly Dictionary<string, List<Type>> Triggers = new();
    private static readonly MethodInfo RegisterRequestMethod = typeof(ActivityVisitor).GetMethod(
        nameof(RegisterRequestEndpoint),
        BindingFlags.Instance | BindingFlags.NonPublic
    )!;

    private string CurrentActivityName = string.Empty;
    private BehaviorStatus[] SupportedStatuses = [];
    
    public void Visit<T>()
    {
        RegisterEventEndpoint<T>(CurrentActivityName);
    }

    public Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> HateoasLinks { get; set; } = new();

    public override Task InitializerAddedAsync<TInitializationEvent>(string activityName, int activityVersion)
    {
        Initializers[activityName] = true;
        
        CurrentActivityName = activityName;
        typeMapper.VisitMappedTypes<TInitializationEvent>(this);
        CurrentActivityName = string.Empty;

        return Task.CompletedTask;
    }

    public override Task AcceptEventNodeAddedAsync<TEvent>(string activityName, int activityVersion, string nodeName,
        string? parentNodeName = null)
    {
        CurrentActivityName = activityName;
        typeMapper.VisitMappedTypes<TEvent>(this);
        CurrentActivityName = string.Empty;

        return Task.CompletedTask;
    }

    public override Task ActivityAddedAsync(string activityName, int activityVersion)
    {
        RegisterStandardEndpoints(activityName);
        RegisterRemainingEndpoints(activityName);
        
        return Task.CompletedTask;
    }

    public override Task ActivityTypeAddedAsync<TActivity>(string activityName, int activityVersion)
    {
        if (typeof(IActivityEndpoints).IsAssignableFrom(typeof(TActivity)))
        {
            var endpointsBuilder = new EndpointsBuilder(routeBuilder, this, interceptor, new ActivityClass(activityName));
            
            var activity = (IActivityEndpoints)StateflowsActivator.CreateUninitializedInstance<TActivity>();
            activity.RegisterEndpoints(endpointsBuilder);
        }
        
        return Task.CompletedTask;
    }

    private void RegisterEventEndpoint<TEvent>(string activityName, IEndpointRouteBuilder activity)
        => activity.RegisterEventEndpoint<TEvent>(interceptor,
            BehaviorType.Activity, activityName, HateoasLinks);

    private void RegisterRequestEndpoint<TRequest, TResponse>(string activityName, IEndpointRouteBuilder activity)
        where TRequest : IRequest<TResponse>
        => activity.RegisterRequestEndpoint<TRequest, TResponse>(interceptor,
            BehaviorType.Activity, activityName, HateoasLinks);
    
    private void RegisterEventEndpoint<TEvent>(string activityName)
    {
        var eventType = typeof(TEvent);
        if (
            eventType.GetCustomAttributes<NoApiMappingAttribute>().Any() ||
            eventType.IsAssignableTo(typeof(SystemEvent)) ||
            eventType.IsAssignableTo(typeof(Exception))
        )
        {
            return;
        }

        if (!Triggers.TryGetValue(activityName, out var triggers))
        {
            triggers = new List<Type>();
            Triggers.Add(activityName, triggers);
        }

        if (!triggers.Contains(typeof(TEvent)))
        {
            triggers.Add(eventType);
            
            if (eventType.IsRequest())
            {
                var responseType = eventType
                    .GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
                    .GetGenericArguments()
                    .First();
                
                var method = RegisterRequestMethod.MakeGenericMethod(eventType, responseType);
                method.Invoke(
                    this,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    [activityName, routeBuilder], 
                    null
                );
            }
            else
            {
                RegisterEventEndpoint<TEvent>(activityName, routeBuilder);
            }
        }
    }

    private void RegisterRemainingEndpoints(string activityName)
    {
        if (Initializers.TryGetValue(activityName, out var hasInitializers) && hasInitializers)
        {
            return;
        }
        
        RegisterEventEndpoint<Initialize>(activityName);
    }

    private static string GetEventName<TEvent>()
        => JsonNamingPolicy.CamelCase.ConvertName(Event<TEvent>.Name.ToShortName());

    private void RegisterStandardEndpoints(string activityName)
    {
        var behaviorClass = new ActivityClass(activityName);
        
        var method = HttpMethods.Get;
        var route = $"/activities/{activityName}";
        if (interceptor.BeforeGetInstancesEndpointDefinition(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(route, [method], async (IStateflowsStorage storage) =>
            {
                BehaviorClass[] actionClasses = [behaviorClass];
                var contextIds = await storage.GetAllContextIdsAsync(actionClasses);
                return Results.Ok(contextIds.Select(id => new { Id = id }));
            })
            .WithTags($"{BehaviorType.Activity} {activityName}");

            interceptor.AfterGetInstancesEndpointDefinition(behaviorClass, method, route, routeHandlerBuilder);
        }

        route = $"/activities/{activityName}/{{instance}}/status";
        method = HttpMethods.Get;
        if (interceptor.BeforeEventEndpointDefinition<ActivityInfoRequest>(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(
                route,
                [method],
                async (
                    string instance,
                    IActivityLocator locator
                ) =>
                {
                    if (locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior))
                    {
                        var result = await behavior.GetStatusAsync([new NoImplicitInitialization()]);
                        // workaround for return code 200 regardless behavior actual status
                        result.Status = EventStatus.Consumed;
                        return result.ToResult([], result.Response, HateoasLinks);
                    }

                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.Activity} {activityName}");

            interceptor.AfterEventEndpointDefinition<ActivityInfoRequest>(behaviorClass, method, route, routeHandlerBuilder);
            
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

        route = $"/activities/{activityName}/{{instance}}/notifications";
        method = HttpMethods.Get;
        if (interceptor.BeforeEventEndpointDefinition<NotificationsRequest>(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(
                route,
                [method],
                async (
                    IActivityLocator locator,
                    string instance,
                    [FromQuery] string[] names,
                    [FromQuery] TimeSpan? period
                ) =>
                {
                    period ??= TimeSpan.FromSeconds(60);
                    if (locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior))
                    {
                        var result = await behavior.GetNotificationsAsync(names, period);
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return ((SendResult)result).ToResult(result.Response.Notifications, behaviorInfo, HateoasLinks);
                    }

                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.Activity} {activityName}");
            
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

        route = $"/activities/{activityName}/{{instance}}/finalize";
        method = HttpMethods.Post;
        if (interceptor.BeforeEventEndpointDefinition<Finalize>(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(
                route,
                [method],
                async (
                    string instance,
                    IActivityLocator locator
                ) =>
                {
                    if (locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior))
                    {
                        var result = await behavior.FinalizeAsync();
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return result.ToResult([], behaviorInfo, HateoasLinks);
                    }
                    
                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.Activity} {activityName}");
            
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

        route = $"/activities/{activityName}/{{instance}}";
        method = HttpMethods.Delete;
        if (interceptor.BeforeEventEndpointDefinition<Reset>(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(
                route,
                [method],
                async (string instance, IActivityLocator locator) =>
                {
                    if (locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior))
                    {
                        var result = await behavior.ResetAsync();
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return result.ToResult([], behaviorInfo, HateoasLinks);
                    }
                    
                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.Activity} {activityName}");
            
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

    public override Task NodeTypeAddedAsync<TNode>(string activityName, int activityVersion, string nodeName)
    {
        if (typeof(IStructuredActivityNodeEndpoints).IsAssignableFrom(typeof(TNode)))
        {
            var behaviorClass = new ActivityClass(activityName);
            
            DependencyInjection.ActivityEndpointBuilders.Add(visitor =>
            {
                var builder = new EndpointsBuilder(routeBuilder, visitor, interceptor, behaviorClass, nodeName);
                
                var node = (IStructuredActivityNodeEndpoints)StateflowsActivator.CreateUninitializedInstance<TNode>();
                node.RegisterEndpoints(builder);
            });
        }

        return Task.CompletedTask;
    }

    public override Task CustomEventAddedAsync<TEvent>(string activityName, int activityVersion, BehaviorStatus[] supportedStatuses)
    {
        CurrentActivityName = activityName;
        typeMapper.VisitMappedTypes<TEvent>(this);
        CurrentActivityName = string.Empty;

        return Task.CompletedTask;
    }
}