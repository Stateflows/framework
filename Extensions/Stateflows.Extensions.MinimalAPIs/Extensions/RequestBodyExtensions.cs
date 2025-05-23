using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Stateflows.Common;
using Stateflows.Common.Classes;

namespace Stateflows.Extensions.MinimalAPIs;

internal static class RequestBodyExtensions
{
    public static void RegisterEventEndpoint<TEvent>(this RouteGroupBuilder routeGroupBuilder, Action<RouteHandlerBuilder> routeHandlerBuilderAction, string behaviorType, string behaviorName, Dictionary<string, List<HateoasLink>> customHateoasLinks)
    {
        var eventType = typeof(TEvent);
        if (Utils.IsEventEmpty(eventType))
        {
            routeHandlerBuilderAction(
                routeGroupBuilder.MapPost("/{instance}/" + Utils.GetEventName<TEvent>(),
                    async (
                        HttpContext context,
                        IServiceProvider serviceProvider,
                        string instance,
                        IBehaviorLocator locator,
                        RequestBody payload,
                        [FromQuery] bool implicitInitialization = true
                    ) =>
                    {
                        var (success, authorizationResult) = await Utils.AuthorizeEventAsync(eventType, serviceProvider, context);
                        if (!success)
                        {
                            return authorizationResult;
                        }

                        if (locator.TryLocateBehavior(new BehaviorId(behaviorType, behaviorName, instance), out var behavior))
                        {
                            var result = await behavior.SendAsync(StateflowsActivator.CreateUninitializedInstance(eventType), implicitInitialization ? [] : [new NoImplicitInitialization()]);
                        
                            var notifications = (await behavior.GetNotificationsAsync(payload.RequestedNotifications)).Response.Notifications.ToArray();
                            var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;

                            return result.ToResult(notifications, behaviorInfo, customHateoasLinks);
                        }
                    
                        return Results.NotFound();
                    }
                )
            );
        }
        else
        {
            routeHandlerBuilderAction(
                routeGroupBuilder.MapPost("/{instance}/" + Utils.GetEventName<TEvent>(),
                    async (
                        HttpContext context,
                        IServiceProvider serviceProvider,
                        string instance,
                        IBehaviorLocator locator,
                        RequestBody<TEvent> payload,
                        [FromQuery] bool implicitInitialization = true
                    ) =>
                    {
                        var (success, authorizationResult) = await Utils.AuthorizeEventAsync(eventType, serviceProvider, context);
                        if (!success)
                        {
                            return authorizationResult;
                        }

                        return locator.TryLocateBehavior(new BehaviorId(behaviorType, behaviorName, instance),
                            out var behavior)
                            ? await payload.SendEndpointAsync(behavior, implicitInitialization, customHateoasLinks)
                            : Results.NotFound();
                    }
                )
            );
        }
    }
    
    public static void RegisterRequestEndpoint<TRequest, TResponse>(this RouteGroupBuilder routeGroupBuilder, Action<RouteHandlerBuilder> routeHandlerBuilderAction, string behaviorType, string behaviorName, Dictionary<string, List<HateoasLink>> customHateoasLinks)
        where TRequest : IRequest<TResponse>
    {
        var eventType = typeof(TRequest);
        if (Utils.IsEventEmpty(eventType))
        {
            routeHandlerBuilderAction(
                routeGroupBuilder.MapPost("/{instance}/" + Utils.GetEventName<TRequest>(),
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
                            var result = await behavior.RequestAsync((TRequest)StateflowsActivator.CreateUninitializedInstance(eventType),
                                    implicitInitialization ? [] : [new NoImplicitInitialization()]);

                            var notifications = (await behavior.GetNotificationsAsync(payload.RequestedNotifications))
                                .Response.Notifications.ToArray();
                            var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()]))
                                .Response;
                            return result.ToResult(notifications, behaviorInfo, customHateoasLinks);
                        }

                        return Results.NotFound();
                    }
                )
            );
        }
        else
        {
            routeHandlerBuilderAction(
                routeGroupBuilder.MapPost("/{instance}/" + Utils.GetEventName<TRequest>(),
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
                )
            );
        }
    }
    
    public static async Task<IResult?> SendEndpointAsync<TEvent>(this RequestBody<TEvent> payload, IBehavior behavior,
        bool implicitInitialization, Dictionary<string, List<HateoasLink>> customHateoasLinks)
    {
        var result = EqualityComparer<TEvent>.Default.Equals(payload.Event, default)
            ? new SendResult(
                EventStatus.Invalid,
                new EventValidation(false, [ new ValidationResult("Event not provided") ])
            )
            : await behavior.SendAsync(payload.Event, implicitInitialization ? [] : [new NoImplicitInitialization()]);
                        
        var notifications = (await behavior.GetNotificationsAsync(payload.RequestedNotifications)).Response.Notifications.ToArray();
        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;

        return result.ToResult(notifications, behaviorInfo, customHateoasLinks);
    }
    
    public static async Task<IResult> RequestEndpointAsync<TRequest, TResponse>(this RequestBody<TRequest> payload, IBehavior behavior,
        bool implicitInitialization, Dictionary<string, List<HateoasLink>> customHateoasLinks)
        where TRequest : IRequest<TResponse>
    {
        var result = EqualityComparer<TRequest>.Default.Equals(payload.Event, default)
            ? new SendResult(
                EventStatus.Invalid,
                new EventValidation(false, [ new ValidationResult("Event not provided") ])
            )
            : await behavior.RequestAsync(payload.Event, implicitInitialization ? [] : [new NoImplicitInitialization()]);
        var notifications = (await behavior.GetNotificationsAsync(payload.RequestedNotifications)).Response.Notifications.ToArray();
        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;

        return result.ToResult(notifications, behaviorInfo, customHateoasLinks);
    }
}