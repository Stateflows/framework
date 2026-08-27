using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Extensions;
using Stateflows.Extensions.MinimalAPIs.Attributes;
using Stateflows.Extensions.MinimalAPIs.Headers;
using Stateflows.StateMachines;

namespace Stateflows.Extensions.MinimalAPIs;

internal static class RequestBodyExtensions
{
    private static async Task<BehaviorInfo> GetBehaviorInfo(this IBehavior behavior)
    {
        var behaviorInfo = behavior.Id.Type switch
        {
            BehaviorType.StateMachine => (await behavior.RequestAsync(new StateMachineInfoRequest(), new Dictionary<string, EventHeader>() { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } })).Response,
            BehaviorType.Activity => (await behavior.RequestAsync(new ActivityInfoRequest(), new Dictionary<string, EventHeader>() { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } })).Response,
            _ => (await behavior.GetStatusAsync(new Dictionary<string, EventHeader>() { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } })).Response
        };
        return behaviorInfo;
    }

    public static void RegisterEventEndpoint<TEvent>(
        this IEndpointRouteBuilder routeBuilder,
        Interceptor interceptor,
        string behaviorType,
        string behaviorName,
        Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> customHateoasLinks,
        bool hasDefaultInstance = false,
        BehaviorStatus[]? supportedStatuses = null
    )
    {
        var eventType = typeof(TEvent);
        var eventName = Utils.GetEventName<TEvent>();
        var watchedNotificationTypes = GetWatchedNotificationTypes(eventType);
        supportedStatuses ??= [BehaviorStatus.Initialized];
        var route = $"/{behaviorType.ToResource()}/{behaviorName}/{{instance}}/{eventName}";
        var method = HttpMethods.Post;
        var behaviorClass = new BehaviorClass(behaviorType, behaviorName);
        if (interceptor.BeforeEventEndpointDefinition<TEvent>(behaviorClass, isDefaultInstance: false, ref method, ref route))
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
                        HttpContext httpContext,
                        RequestBody payload,
                        [FromQuery] bool implicitInitialization = true,
                        [FromQuery] bool stream = false
                    ) =>
                    {
                        var (success, authorizationResult) =
                            await Utils.AuthorizeEventAsync(eventType, serviceProvider, context);
                        if (!success)
                        {
                            return authorizationResult;
                        }

                        if (!locator.TryLocateBehavior(new BehaviorId(behaviorType, behaviorName, instance),
                                out var behavior))
                        {
                            return Results.NotFound();
                        }

                        MergeNotificationNames(watchedNotificationTypes, payload);
                        
                        var processingAction = () => payload.SendEndpointAsync(method, route,
                            StateflowsActivator.CreateUninitializedInstance<TEvent>(), behavior, implicitInitialization,
                            customHateoasLinks, context);

                        if (stream)
                        {
                            await SSENotificationsWatchAsync(httpContext, behavior, payload, processingAction);

                            return Results.Empty;
                        }

                        return await processingAction();
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
                        HttpContext httpContext,
                        RequestBody<TEvent> payload,
                        [FromQuery] bool implicitInitialization = true,
                        [FromQuery] bool stream = false
                    ) =>
                    {
                        var (success, authorizationResult) =
                            await Utils.AuthorizeEventAsync(eventType, serviceProvider, context);
                        if (!success)
                        {
                            return authorizationResult;
                        }

                        if (!locator.TryLocateBehavior(new BehaviorId(behaviorType, behaviorName, instance),
                                out var behavior))
                        {
                            return Results.NotFound();
                        }

                        MergeNotificationNames(watchedNotificationTypes, payload);
                        
                        var processingAction = () => payload.SendEndpointAsync(method, route, payload.Event, behavior, implicitInitialization,
                            customHateoasLinks, context);

                        if (stream)
                        {
                            await SSENotificationsWatchAsync(httpContext, behavior, payload, processingAction);

                            return Results.Empty;
                        }

                        return await processingAction();
                    }
                );

            routeHandlerBuilder.WithTags($"{behaviorClass.Type} {behaviorClass.Name}");

            interceptor.AfterEventEndpointDefinition<TEvent>(behaviorClass, isDefaultInstance: false, method, route, routeHandlerBuilder);

            customHateoasLinks.AddLink(
                behaviorClass.Name,
                new HateoasLink()
                {
                    Rel = eventName.ToShortName().ToCamelCase(),
                    Href = route,
                    Method = method
                },
                supportedStatuses,
                eventName,
                "standard:event"
            );
        }

        if (!hasDefaultInstance)
        {
            return;
        }

        if (typeof(TEvent) == typeof(Initialize))
        {
            return;
        }

        var defaultInstanceRoute = $"/{behaviorType.ToResource()}/{behaviorName}/{eventName}";
        if (interceptor.BeforeEventEndpointDefinition<TEvent>(behaviorClass, isDefaultInstance: true, ref method, ref defaultInstanceRoute))
        {
            var routeHandlerBuilder = Utils.IsEventEmpty(eventType)
                ? routeBuilder.MapMethods(
                    defaultInstanceRoute,
                    [method],
                    async (
                        HttpContext context,
                        IServiceProvider serviceProvider,
                        IBehaviorLocator locator,
                        HttpContext httpContext,
                        RequestBody payload,
                        [FromQuery] bool stream = false
                    ) =>
                    {
                        var (success, authorizationResult) =
                            await Utils.AuthorizeEventAsync(eventType, serviceProvider, context);
                        if (!success)
                        {
                            return authorizationResult;
                        }

                        if (!locator.TryLocateBehavior(new BehaviorId(behaviorType, behaviorName, string.Empty),
                                out var behavior))
                        {
                            return Results.NotFound();
                        }

                        MergeNotificationNames(watchedNotificationTypes, payload);

                        var processingAction = () => payload.SendEndpointAsync(method, route,
                            StateflowsActivator.CreateUninitializedInstance<TEvent>(), behavior, true,
                            customHateoasLinks, context);

                        if (stream)
                        {
                            await SSENotificationsWatchAsync(httpContext, behavior, payload, processingAction);

                            return Results.Empty;
                        }

                        return await processingAction();
                    }
                )
                : routeBuilder.MapMethods(
                    defaultInstanceRoute,
                    [method],
                    async (
                        HttpContext context,
                        IServiceProvider serviceProvider,
                        IBehaviorLocator locator,
                        HttpContext httpContext,
                        RequestBody<TEvent> payload,
                        [FromQuery] bool stream = false
                    ) =>
                    {
                        var (success, authorizationResult) = await Utils.AuthorizeEventAsync(eventType, serviceProvider, context);
                        if (!success)
                        {
                            return authorizationResult;
                        }

                        if (!locator.TryLocateBehavior(new BehaviorId(behaviorType, behaviorName, string.Empty),
                                out var behavior))
                        {
                            return Results.NotFound();
                        }

                        MergeNotificationNames(watchedNotificationTypes, payload);

                        var processingAction = () => payload.SendEndpointAsync(method, route, payload.Event, behavior, true,
                            customHateoasLinks, context);

                        if (stream)
                        {
                            await SSENotificationsWatchAsync(httpContext, behavior, payload, processingAction);

                            return Results.Empty;
                        }

                        return await processingAction();
                    }
                );

            routeHandlerBuilder.WithTags($"{behaviorClass.Type} {behaviorClass.Name}");

            interceptor.AfterEventEndpointDefinition<TEvent>(behaviorClass, isDefaultInstance: true, method, defaultInstanceRoute, routeHandlerBuilder);

            customHateoasLinks.AddLink(
                behaviorClass.Name,
                new HateoasLink()
                {
                    Rel = eventName.ToShortName().ToCamelCase(),
                    Href = defaultInstanceRoute,
                    Method = method
                },
                [BehaviorStatus.Initialized],
                eventName,
                "default:event"
            );
        }
    }

    private static void MergeNotificationNames(string[] watchedNotificationTypes, RequestBody payload)
    {
        if (watchedNotificationTypes.Any())
        {
            payload.RequestedNotifications = (
                payload.RequestedNotifications != null
                    ? [
                        ..payload.RequestedNotifications,
                        ..watchedNotificationTypes
                    ]
                    : watchedNotificationTypes
            );
        }
    }

    private static string[] GetWatchedNotificationTypes(Type eventType)
    {
        var notificationWatchType = typeof(NotificationWatchAttribute<>);
        var watchedNotificationTypes = eventType
            .GetCustomAttributes(true)
            .Where(a =>
            {
                var t = a.GetType();
                return t.IsGenericType && t.IsSubclassOfRawGeneric(notificationWatchType);
            })
            .Select(a => a.GetType().GetGenericArguments().First().GetReadableName(TypedElements.Events))
            .Distinct()
            .ToArray();
        return watchedNotificationTypes;
    }

    public static void RegisterRequestEndpoint<TRequest, TResponse>(
        this IEndpointRouteBuilder routeBuilder,
        Interceptor interceptor,
        string behaviorType,
        string behaviorName,
        Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> customHateoasLinks,
        bool hasDefaultInstance = false,
        BehaviorStatus[]? supportedStatuses = null
    )
        where TRequest : IRequest<TResponse>
    {
        supportedStatuses ??= [BehaviorStatus.Initialized];
        var eventType = typeof(TRequest);
        var eventName = Utils.GetEventName<TRequest>();
        var watchedNotificationTypes = GetWatchedNotificationTypes(eventType);
        var route = $"/{behaviorType.ToResource()}/{behaviorName}/{{instance}}/" + Utils.GetEventName<TRequest>();
        var method = HttpMethods.Post;
        var behaviorClass = new BehaviorClass(behaviorType, behaviorName);
        if (interceptor.BeforeEventEndpointDefinition<TRequest>(behaviorClass, isDefaultInstance: false, ref method, ref route))
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
                        HttpContext httpContext,
                        RequestBody payload,
                        [FromQuery] bool implicitInitialization = true,
                        [FromQuery] bool stream = false
                    ) =>
                    {
                        var (success, authorizationResult) =
                            await Utils.AuthorizeEventAsync(eventType, serviceProvider, context);
                        if (!success)
                        {
                            return authorizationResult;
                        }

                        if (!locator.TryLocateBehavior(new BehaviorId(behaviorType, behaviorName, instance),
                                out var behavior))
                        {
                            return Results.NotFound();
                        }

                        MergeNotificationNames(watchedNotificationTypes, payload);
                        
                        var processingAction = () => payload.RequestEndpointAsync<TRequest, TResponse>(
                            method, route, StateflowsActivator.CreateUninitializedInstance<TRequest>(), behavior,
                            implicitInitialization, customHateoasLinks, context
                        );

                        if (stream)
                        {
                            await SSENotificationsWatchAsync(httpContext, behavior, payload, processingAction);

                            return Results.Empty;
                        }

                        return await processingAction();
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
                        HttpContext httpContext,
                        RequestBody<TRequest> payload,
                        [FromQuery] bool implicitInitialization = true,
                        [FromQuery] bool stream = false
                    ) =>
                    {
                        var (success, authorizationResult) = await Utils.AuthorizeEventAsync(eventType, serviceProvider, context);
                        if (!success)
                        {
                            return authorizationResult;
                        }

                        if (!locator.TryLocateBehavior(new BehaviorId(behaviorType, behaviorName, instance),
                                out var behavior))
                        {
                            return Results.NotFound();
                        }

                        MergeNotificationNames(watchedNotificationTypes, payload);

                        var processingAction = () => payload.RequestEndpointAsync<TRequest, TResponse>(method, route, payload.Event, behavior,
                            implicitInitialization, customHateoasLinks, context);

                        if (stream)
                        {
                            await SSENotificationsWatchAsync(httpContext, behavior, payload, processingAction);

                            return Results.Empty;
                        }

                        return await processingAction();
                    }
                );

            routeHandlerBuilder.WithTags($"{behaviorClass.Type} {behaviorClass.Name}");

            interceptor.AfterEventEndpointDefinition<TRequest>(behaviorClass, isDefaultInstance: false, method, route, routeHandlerBuilder);

            customHateoasLinks.AddLink(
                behaviorClass.Name,
                new HateoasLink()
                {
                    Rel = eventName.ToShortName().ToCamelCase(),
                    Href = route,
                    Method = method
                },
                supportedStatuses,
                eventName,
                "standard:event"
            );
        }

        if (!hasDefaultInstance)
        {
            return;
        }

        if (interceptor.BeforeEventEndpointDefinition<TRequest>(behaviorClass, isDefaultInstance: true, ref method, ref route))
        {
            var defaultInstanceRoute = $"/{behaviorType.ToResource()}/{behaviorName}/{Utils.GetEventName<TRequest>()}";

            var routeHandlerBuilder = Utils.IsEventEmpty(eventType)
                ? routeBuilder.MapMethods(
                    defaultInstanceRoute,
                    [method],
                    async (
                        HttpContext context,
                        IServiceProvider serviceProvider,
                        IBehaviorLocator locator,
                        HttpContext httpContext,
                        RequestBody payload,
                        [FromQuery] bool stream = false
                    ) =>
                    {
                        var (success, authorizationResult) =
                            await Utils.AuthorizeEventAsync(eventType, serviceProvider, context);
                        if (!success)
                        {
                            return authorizationResult;
                        }

                        if (!locator.TryLocateBehavior(new BehaviorId(behaviorType, behaviorName, string.Empty), out var behavior))
                        {
                            return Results.NotFound();
                        }

                        MergeNotificationNames(watchedNotificationTypes, payload);

                        var processingAction = () => payload.RequestEndpointAsync<TRequest, TResponse>(
                            method, route, StateflowsActivator.CreateUninitializedInstance<TRequest>(), behavior, true,
                            customHateoasLinks, context);

                        if (stream)
                        {
                            await SSENotificationsWatchAsync(httpContext, behavior, payload, processingAction);

                            return Results.Empty;
                        }

                        return await processingAction();
                    }
                )
                : routeBuilder.MapMethods(
                    defaultInstanceRoute,
                    [method],
                    async (
                        HttpContext context,
                        IServiceProvider serviceProvider,
                        IBehaviorLocator locator,
                        HttpContext httpContext,
                        RequestBody<TRequest> payload,
                        [FromQuery] bool stream = false
                    ) =>
                    {
                        var (success, authorizationResult) = await Utils.AuthorizeEventAsync(eventType, serviceProvider, context);
                        if (!success)
                        {
                            return authorizationResult;
                        }

                        if (!locator.TryLocateBehavior(new BehaviorId(behaviorType, behaviorName, string.Empty),
                                out var behavior))
                        {
                            return Results.NotFound();
                        }

                        MergeNotificationNames(watchedNotificationTypes, payload);

                        var processingAction = () => payload.RequestEndpointAsync<TRequest, TResponse>(method, route, payload.Event, behavior, true, customHateoasLinks, context);

                        if (stream)
                        {
                            await SSENotificationsWatchAsync(httpContext, behavior, payload, processingAction);

                            return Results.Empty;
                        }

                        return await processingAction();
                    }
                );

            routeHandlerBuilder.WithTags($"{behaviorClass.Type} {behaviorClass.Name}");

            interceptor.AfterEventEndpointDefinition<TRequest>(behaviorClass, isDefaultInstance: true, method, route, routeHandlerBuilder);

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
                "default:event"
            );
        }
    }

    private static async Task SSENotificationsWatchAsync(HttpContext httpContext, IBehavior behavior, RequestBody payload, Func<Task> processingAction)
    {
        httpContext.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");

        var n = payload.RequestedNotifications?.ToList() ?? [];
        n.Add("String");
        
        await using var watcher = await behavior.WatchAsync(
            n.ToArray(),
            async eventHolder => await httpContext.WriteEventAsync(eventHolder)
        );

        var processingTask = processingAction();

        while (!httpContext.RequestAborted.IsCancellationRequested)
        {
            await Task.Delay(1000);
        }

        await processingTask;
    }

    private static async Task<IResult?> SendEndpointAsync<TEvent>(this RequestBody payload, string method, string path, TEvent @event, IBehavior behavior,
        bool implicitInitialization, Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> customHateoasLinks, HttpContext context)
    {
        var notifications = new List<EventHolder>();
        await behavior.WatchAsync(
            payload.RequestedNotifications,
            notifications.Add
        );

        if (EqualityComparer<TEvent>.Default.Equals(@event, default))
        {
            var result = new SendResult(
                EventStatus.Invalid,
                new EventValidation(false, [new ValidationResult("Event not provided")])
            );

            var behaviorInfo = await behavior.GetBehaviorInfo();
            return result.ToResult(method ?? "[no method]", path ?? "[no path]", [], behaviorInfo, customHateoasLinks);
        }
        else
        {
            var result = await behavior.SendAsync(
                @event,
                implicitInitialization
                    ? new Dictionary<string, EventHeader>
                    {
                        { nameof(HttpContextHeader), new HttpContextHeader() { Context = context } }
                    }
                    : new Dictionary<string, EventHeader>
                    {
                        { nameof(NoImplicitInitialization), new NoImplicitInitialization() },
                        { nameof(HttpContextHeader), new HttpContextHeader() { Context = context } }
                    }
            );
            var behaviorInfo = await behavior.GetBehaviorInfo();
            
            // var notifications = payload.RequestedNotifications is { Length: > 0 } && result.Status == EventStatus.Consumed
            //     ? (await behavior.GetNotificationsAsync(payload.RequestedNotifications, lastNotificationsCheck)).ToArray()
            //     : [];

            return result.ToResult(method ?? "[no method]", path ?? "[no path]", notifications, behaviorInfo, customHateoasLinks);
        }
    }

    private static async Task<IResult> RequestEndpointAsync<TRequest, TResponse>(this RequestBody payload, string method, string path, TRequest request, IBehavior behavior,
        bool implicitInitialization, Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> customHateoasLinks, HttpContext context)
        where TRequest : IRequest<TResponse>
    {
        var notifications = new List<EventHolder>();
        await behavior.WatchAsync(
            payload.RequestedNotifications,
            notifications.Add
        );

        if (EqualityComparer<TRequest>.Default.Equals(request, default))
        {
            var result = new SendResult(
                EventStatus.Invalid,
                new EventValidation(false, [new ValidationResult("Event not provided")])
            );

            var behaviorInfo = await behavior.GetBehaviorInfo();

            return result.ToResult(method ?? "[no method]", path ?? "[no path]", [], behaviorInfo, customHateoasLinks);
        }
        else
        {
            var requestResult = await behavior.RequestAsync(
                request,
                implicitInitialization
                    ? new Dictionary<string, EventHeader>
                    {
                        { nameof(HttpContextHeader), new HttpContextHeader() { Context = context } }
                    }
                    : new Dictionary<string, EventHeader>
                    {
                        { nameof(NoImplicitInitialization), new NoImplicitInitialization() },
                        { nameof(HttpContextHeader), new HttpContextHeader() { Context = context } }
                    }
            );
            var behaviorInfo = await behavior.GetBehaviorInfo();

            // var notifications = payload.RequestedNotifications is { Length: > 0 } && requestResult.Status == EventStatus.Consumed
            //     ? (await behavior.GetNotificationsAsync(payload.RequestedNotifications, lastNotificationsCheck)).ToArray()
            //     : [];

            return requestResult.ToResult(method ?? "[no method]", path ?? "[no path]", notifications, behaviorInfo, customHateoasLinks);
        }
    }
}