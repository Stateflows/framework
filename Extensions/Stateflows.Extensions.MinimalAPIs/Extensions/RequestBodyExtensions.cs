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
                            ? await payload.SendEndpointAsync(StateflowsActivator.CreateUninitializedInstance<TEvent>(), behavior, implicitInitialization, customHateoasLinks, context)
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
                            ? await payload.SendEndpointAsync(payload.Event, behavior, implicitInitialization, customHateoasLinks, context)
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
        var route = $"/{behaviorType.ToResource()}/{behaviorName}/{{instance}}/" + Utils.GetEventName<TRequest>();
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
                            ? await payload.RequestEndpointAsync<TRequest, TResponse>(StateflowsActivator.CreateUninitializedInstance<TRequest>(), behavior, implicitInitialization, customHateoasLinks, context)
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
                            ? await payload.RequestEndpointAsync<TRequest, TResponse>(payload.Event, behavior, implicitInitialization, customHateoasLinks, context)
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
        bool implicitInitialization, Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> customHateoasLinks, HttpContext context)
    {
        var lastNotificationsCheck = DateTime.Now;

        if (EqualityComparer<TEvent>.Default.Equals(@event, default))
        {
            var result = new SendResult(
                EventStatus.Invalid,
                new EventValidation(false, [new ValidationResult("Event not provided")])
            );
            
            var behaviorInfo = await behavior.GetBehaviorInfo();
            return result.ToResult([], behaviorInfo, customHateoasLinks);
        }
        else
        {
            var compoundResult = await behavior.SendCompoundAsync(b =>
                {
                    b.Add(@event, implicitInitialization ? [] : [new NoImplicitInitialization()]);

                    switch (behavior.Id.Type)
                    {
                        case BehaviorType.StateMachine:
                            b.Add(
                                new StateMachineInfoRequest(),
                                implicitInitialization
                                    ? [new ForcedExecution()]
                                    : [new NoImplicitInitialization(), new ForcedExecution()]
                            );
                            break;
                        case BehaviorType.Activity:
                            b.Add(
                                new ActivityInfoRequest(),
                                implicitInitialization
                                    ? [new ForcedExecution()]
                                    : [new NoImplicitInitialization(), new ForcedExecution()]
                            );
                            break;
                        case BehaviorType.Action:
                            b.Add(
                                new BehaviorInfoRequest(),
                                implicitInitialization
                                    ? [new ForcedExecution()]
                                    : [new NoImplicitInitialization(), new ForcedExecution()]
                            );
                            break;
                    }
                }
            );

            var result = compoundResult.Response.Results.First();
            var behaviorInfo = (BehaviorInfo)compoundResult.Response.Results.Last().Response.BoxedPayload;
            
            var notifications = payload.RequestedNotifications is { Length: > 0 } && result.Status == EventStatus.Consumed
                ? (await behavior.GetNotificationsAsync(payload.RequestedNotifications, lastNotificationsCheck)).ToArray()
                : [];
            
            return result.ToResult(notifications, behaviorInfo, customHateoasLinks);
        }
    }
    
    private static async Task<IResult> RequestEndpointAsync<TRequest, TResponse>(this RequestBody payload, TRequest request, IBehavior behavior,
        bool implicitInitialization, Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> customHateoasLinks, HttpContext context)
        where TRequest : IRequest<TResponse>
    {
        var lastNotificationsCheck = DateTime.Now;

        if (EqualityComparer<TRequest>.Default.Equals(request, default))
        {
            var result = new SendResult(
                EventStatus.Invalid,
                new EventValidation(false, [new ValidationResult("Event not provided")])
            );
            
            var behaviorInfo = await behavior.GetBehaviorInfo();
            
            return result.ToResult([], behaviorInfo, customHateoasLinks);
        }
        else
        {
            var compoundResult = await behavior.SendCompoundAsync(b =>
                {
                    b.Add(request, implicitInitialization ? [] : [new NoImplicitInitialization()]);

                    switch (behavior.Id.Type)
                    {
                        case BehaviorType.StateMachine:
                            b.Add(
                                new StateMachineInfoRequest(),
                                implicitInitialization
                                    ? [new ForcedExecution()]
                                    : [new NoImplicitInitialization(), new ForcedExecution()]
                            );
                            break;
                        case BehaviorType.Activity:
                            b.Add(
                                new ActivityInfoRequest(),
                                implicitInitialization
                                    ? [new ForcedExecution()]
                                    : [new NoImplicitInitialization(), new ForcedExecution()]
                            );
                            break;
                        case BehaviorType.Action:
                            b.Add(
                                new BehaviorInfoRequest(),
                                implicitInitialization
                                    ? [new ForcedExecution()]
                                    : [new NoImplicitInitialization(), new ForcedExecution()]
                            );
                            break;
                    }
                }
            );

            var result = compoundResult.Response.Results.First();
            var requestResult = new RequestResult<TResponse>(result);
            var behaviorInfo = (BehaviorInfo)compoundResult.Response.Results.Last().Response.BoxedPayload;

            var notifications =
                payload.RequestedNotifications is { Length: > 0 } && requestResult.Status == EventStatus.Consumed
                    ? (await behavior.GetNotificationsAsync(payload.RequestedNotifications, lastNotificationsCheck))
                    .ToArray()
                    : [];

            return requestResult.ToResult(notifications, behaviorInfo, customHateoasLinks);
        }
    }
}