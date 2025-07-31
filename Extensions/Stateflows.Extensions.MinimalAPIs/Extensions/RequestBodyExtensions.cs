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

                        if (locator.TryLocateBehavior(new BehaviorId(behaviorType, behaviorName, instance),
                                out var behavior))
                        {
                            var result = await behavior.SendAsync(
                                StateflowsActivator.CreateUninitializedInstance(eventType),
                                implicitInitialization ? [] : [new NoImplicitInitialization()]);

                            var notifications = (await behavior.GetNotificationsAsync(payload.RequestedNotifications))
                                .Response.Notifications.ToArray();
                            var behaviorInfo = await behavior.GetBehaviorInfo();

                            return result.ToResult(notifications, behaviorInfo, customHateoasLinks);
                        }

                        return Results.NotFound();
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
                            ? await payload.SendEndpointAsync(behavior, implicitInitialization, customHateoasLinks)
                            : Results.NotFound();
                    }
                );

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

    public static void RegisterRequestEndpoint<TRequest, TResponse>(this IEndpointRouteBuilder routeBuilder, Interceptor interceptor, string behaviorType, string behaviorName, Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> customHateoasLinks)
        where TRequest : IRequest<TResponse>
    {
        var eventType = typeof(TRequest);
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

                        if (locator.TryLocateBehavior(new BehaviorId(behaviorType, behaviorName, instance),
                                out var behavior))
                        {
                            var result = await behavior.RequestAsync(
                                (TRequest)StateflowsActivator.CreateUninitializedInstance(eventType),
                                implicitInitialization ? [] : [new NoImplicitInitialization()]);

                            var notifications = (await behavior.GetNotificationsAsync(payload.RequestedNotifications))
                                .Response.Notifications.ToArray();
                            var behaviorInfo = await behavior.GetBehaviorInfo();
                            
                            return result.ToResult(notifications, behaviorInfo, customHateoasLinks);
                        }

                        return Results.NotFound();
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
                            ? await payload.RequestEndpointAsync<TRequest, TResponse>(behavior, implicitInitialization, customHateoasLinks)
                            : Results.NotFound();
                    }
                );

            interceptor.AfterEventEndpointDefinition<TRequest>(behaviorClass, method, route, routeHandlerBuilder);
        }
    }
    
    public static async Task<IResult?> SendEndpointAsync<TEvent>(this RequestBody<TEvent> payload, IBehavior behavior,
        bool implicitInitialization, Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> customHateoasLinks)
    {
        var result = EqualityComparer<TEvent>.Default.Equals(payload.Event, default)
            ? new SendResult(
                EventStatus.Invalid,
                new EventValidation(false, [ new ValidationResult("Event not provided") ])
            )
            : await behavior.SendAsync(payload.Event, implicitInitialization ? [] : [new NoImplicitInitialization()]);
                        
        var notifications = (await behavior.GetNotificationsAsync(payload.RequestedNotifications ?? []))?.Response?.Notifications?.ToArray() ?? [];
        var behaviorInfo = await behavior.GetBehaviorInfo();

        return result.ToResult(notifications, behaviorInfo, customHateoasLinks);
    }
    
    public static async Task<IResult> RequestEndpointAsync<TRequest, TResponse>(this RequestBody<TRequest> payload, IBehavior behavior,
        bool implicitInitialization, Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> customHateoasLinks)
        where TRequest : IRequest<TResponse>
    {
        var result = EqualityComparer<TRequest>.Default.Equals(payload.Event, default)
            ? new SendResult(
                EventStatus.Invalid,
                new EventValidation(false, [ new ValidationResult("Event not provided") ])
            )
            : await behavior.RequestAsync(payload.Event, implicitInitialization ? [] : [new NoImplicitInitialization()]);
        var notifications = (await behavior.GetNotificationsAsync(payload.RequestedNotifications ?? [])).Response?.Notifications?.ToArray() ?? [];
        var behaviorInfo = await behavior.GetBehaviorInfo();

        return result.ToResult(notifications, behaviorInfo, customHateoasLinks);
    }
}