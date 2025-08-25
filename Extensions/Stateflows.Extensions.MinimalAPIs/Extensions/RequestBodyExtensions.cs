using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.StateMachines;

namespace Stateflows.Extensions.MinimalAPIs;

internal static class RequestBodyExtensions
{
    private static ICompoundRequestBuilder AddBehaviorInfoRequest(this ICompoundRequestBuilder builder, BehaviorId behaviorId)
    {
        switch (behaviorId.Type) 
        {
            case BehaviorType.StateMachine:
                builder.Add(new StateMachineInfoRequest(), [new NoImplicitInitialization()]);
                break;
                        
            case BehaviorType.Activity:
                builder.Add(new ActivityInfoRequest(), [new NoImplicitInitialization()]);
                break;
                        
            default:
                builder.Add(new BehaviorInfoRequest(), [new NoImplicitInitialization()]);
                break;
        };
        
        return builder;
    }

    private static BehaviorInfo GetBehaviorInfo(this EventHolder holder, BehaviorId behaviorId)
        => behaviorId.Type switch
        {
            BehaviorType.StateMachine => ((EventHolder<StateMachineInfo>)holder).Payload,
            BehaviorType.Activity => ((EventHolder<ActivityInfo>)holder).Payload,
            _ => ((EventHolder<BehaviorInfo>)holder).Payload,
        };
    
    private static async Task<BehaviorInfo> GetBehaviorInfo(this IBehavior behavior)
    {
        var behaviorInfo = behavior.Id.Type switch
        {
            BehaviorType.StateMachine => (await behavior.RequestAsync(new StateMachineInfoRequest(), [new NoImplicitInitialization()])).Response,
            BehaviorType.Activity => (await behavior.RequestAsync(new ActivityInfoRequest(), [new NoImplicitInitialization()])).Response,
            _ => (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response
        };
        return behaviorInfo;
    }
    
    public static void RegisterEventEndpoint<TEvent>(this IEndpointRouteBuilder routeBuilder, Interceptor interceptor, string behaviorType, string behaviorName, Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> customHateoasLinks)
    {
        var eventType = typeof(TEvent);
        var eventName = Utils.GetEventName<TEvent>();
        var route = $"/{behaviorType.ToResource()}/{behaviorName}/{{instance}}/{eventName}";
        var method = HttpMethods.Post;
        var behaviorClass = new BehaviorClass(behaviorType, behaviorName);
        if (interceptor.BeforeEventEndpointDefinition<TEvent>(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = Utils.IsEventEmpty(eventType)
                ? routeBuilder.MapMethods(
                    route,
                    [method],
                    async (
                        HttpContext context,
                        IServiceProvider serviceProvider,
                        string instance,
                        IBehaviorLocator locator,
                        RequestBody payload,
                        [FromQuery] bool implicitInitialization = true
                    ) =>
                    {
                        var (success, authorizationResult) =
                            await Utils.AuthorizeEventAsync(eventType, serviceProvider, context);
                        if (!success)
                        {
                            return authorizationResult;
                        }
                        
                        return locator.TryLocateBehavior(new BehaviorId(behaviorType, behaviorName, instance),
                            out var behavior)
                            ? await payload.SendEndpointAsync(StateflowsActivator.CreateUninitializedInstance<TEvent>(), behavior, implicitInitialization, customHateoasLinks)
                            : Results.NotFound();
                    }
                )
                : routeBuilder.MapMethods(
                    route,
                    [method],
                    async (
                        HttpContext context,
                        IServiceProvider serviceProvider,
                        string instance,
                        IBehaviorLocator locator,
                        RequestBody<TEvent> payload,
                        [FromQuery] bool implicitInitialization = true
                    ) =>
                    {
                        var (success, authorizationResult) =
                            await Utils.AuthorizeEventAsync(eventType, serviceProvider, context);
                        if (!success)
                        {
                            return authorizationResult;
                        }

                        return locator.TryLocateBehavior(new BehaviorId(behaviorType, behaviorName, instance),
                            out var behavior)
                            ? await payload.SendEndpointAsync(payload.Event, behavior, implicitInitialization, customHateoasLinks)
                            : Results.NotFound();
                    }
                );

            routeHandlerBuilder.WithTags($"{behaviorClass.Type} {behaviorClass.Name}");

            interceptor.AfterEventEndpointDefinition<TEvent>(behaviorClass, method, route, routeHandlerBuilder);
            
            customHateoasLinks.AddLink(
                behaviorClass.Name,
                new HateoasLink()
                {
                    Rel = eventName.ToShortName().ToCamelCase(),
                    Href = route,
                    Method = method
                },
                [BehaviorStatus.Initialized],
                eventName,
                "event"
            );
        }
    }

    public static void RegisterRequestEndpoint<TRequest, TResponse>(this IEndpointRouteBuilder routeBuilder, Interceptor interceptor, string behaviorType, string behaviorName, Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> customHateoasLinks)
        where TRequest : IRequest<TResponse>
    {
        var eventType = typeof(TRequest);
        var eventName = Utils.GetEventName<TRequest>();
        var route = $"{behaviorType.ToResource()}/{behaviorName}/{{instance}}/" + Utils.GetEventName<TRequest>();
        var method = HttpMethods.Post;
        var behaviorClass = new BehaviorClass(behaviorType, behaviorName);
        if (interceptor.BeforeEventEndpointDefinition<TRequest>(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = Utils.IsEventEmpty(eventType)
                ? routeBuilder.MapMethods(
                    route,
                    [method],
                    async (
                        HttpContext context,
                        IServiceProvider serviceProvider,
                        string instance,
                        IBehaviorLocator locator,
                        RequestBody payload,
                        [FromQuery] bool implicitInitialization = true
                    ) =>
                    {
                        var (success, authorizationResult) =
                            await Utils.AuthorizeEventAsync(eventType, serviceProvider, context);
                        if (!success)
                        {
                            return authorizationResult;
                        }

                        return locator.TryLocateBehavior(new BehaviorId(behaviorType, behaviorName, instance), out var behavior)
                            ? await payload.RequestEndpointAsync<TRequest, TResponse>(StateflowsActivator.CreateUninitializedInstance<TRequest>(), behavior, implicitInitialization, customHateoasLinks)
                            : Results.NotFound();
                    }
                )
                : routeBuilder.MapMethods(
                    route,
                    [method],
                    async (
                        HttpContext context,
                        IServiceProvider serviceProvider,
                        string instance,
                        IBehaviorLocator locator,
                        RequestBody<TRequest> payload,
                        [FromQuery] bool implicitInitialization = true
                    ) =>
                    {
                        var (success, authorizationResult) = await Utils.AuthorizeEventAsync(eventType, serviceProvider, context);
                        if (!success)
                        {
                            return authorizationResult;
                        }

                        return locator.TryLocateBehavior(new BehaviorId(behaviorType, behaviorName, instance), out var behavior)
                            ? await payload.RequestEndpointAsync<TRequest, TResponse>(payload.Event, behavior, implicitInitialization, customHateoasLinks)
                            : Results.NotFound();
                    }
                );

            routeHandlerBuilder.WithTags($"{behaviorClass.Type} {behaviorClass.Name}");

            interceptor.AfterEventEndpointDefinition<TRequest>(behaviorClass, method, route, routeHandlerBuilder);
            
            customHateoasLinks.AddLink(
                behaviorClass.Name,
                new HateoasLink()
                {
                    Rel = eventName.ToShortName().ToCamelCase(),
                    Href = route,
                    Method = method
                },
                [BehaviorStatus.Initialized],
                eventName,
                "event"
            );
        }
    }
    
    private static async Task<IResult?> SendEndpointAsync<TEvent>(this RequestBody payload, TEvent @event, IBehavior behavior,
        bool implicitInitialization, Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> customHateoasLinks)
    {
        var lastNotificationsCheck = DateTime.Now;
        
        var result = EqualityComparer<TEvent>.Default.Equals(@event, default)
            ? new SendResult(
                EventStatus.Invalid,
                new EventValidation(false, [ new ValidationResult("Event not provided") ])
            )
            : await behavior.SendAsync(@event, implicitInitialization ? [] : [new NoImplicitInitialization()]);
        
        var notifications = result.Status != EventStatus.Invalid
            ? (await behavior.GetNotificationsAsync(payload.RequestedNotifications, lastNotificationsCheck)).ToArray()
            : [];
        
        var behaviorInfo = await behavior.GetBehaviorInfo();
        
        return result.ToResult(notifications, behaviorInfo, customHateoasLinks);
    }
    
    private static async Task<IResult> RequestEndpointAsync<TRequest, TResponse>(this RequestBody payload, TRequest request, IBehavior behavior,
        bool implicitInitialization, Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> customHateoasLinks)
        where TRequest : IRequest<TResponse>
    {
        var lastNotificationsCheck = DateTime.Now;
        
        var result = EqualityComparer<TRequest>.Default.Equals(request, default)
            ? new RequestResult<TResponse>(
                new EventHolder<TRequest>(),
                EventStatus.Invalid,
                new EventValidation(false, [ new ValidationResult("Event not provided") ])
            )
            : await behavior.RequestAsync(request, implicitInitialization ? [] : [new NoImplicitInitialization()]);
        
        var notifications = result.Status != EventStatus.Invalid
            ? (await behavior.GetNotificationsAsync(payload.RequestedNotifications, lastNotificationsCheck)).ToArray()
            : [];
        
        var behaviorInfo = await behavior.GetBehaviorInfo();
        
        return result.ToResult(notifications, behaviorInfo, customHateoasLinks);
    }
}