using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;
using Stateflows.Actions;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;

namespace Stateflows.Extensions.MinimalAPIs;

internal class ActionVisitor(IEndpointRouteBuilder routeBuilder, Interceptor interceptor)
    : Actions.ActionVisitor, IBehaviorClassVisitor
{
    public IEndpointRouteBuilder RouteBuilder => routeBuilder;
    public Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> HateoasLinks { get; set; } = new();
    private BehaviorClass? OwnerClass = null;
    public bool HasDefaultInstance { get; private set; } = false;

    public override Task ActionAddingAsync(string actionName, int actionVersion, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false)
    {
        OwnerClass = ownerClass;
        HasDefaultInstance = hasDefaultInstance;
        return Task.CompletedTask;
    }

    public override Task ActionAddedAsync(string actionName, int actionVersion)
    {
        if (OwnerClass != null)
        {
            return Task.CompletedTask;
        }

        RegisterStandardEndpoints(actionName, routeBuilder);

        if (HasDefaultInstance)
        {
            RegisterDefaultInstanceEndpoints(actionName, routeBuilder);
        }
        return Task.CompletedTask;
    }

    private static string GetEventName<TEvent>()
        => JsonNamingPolicy.CamelCase.ConvertName(Event<TEvent>.Name.ToShortName());

    private void RegisterStandardEndpoints(string actionName, IEndpointRouteBuilder action)
    {
        var behaviorClass = new ActionClass(actionName);

        var method = HttpMethods.Get;
        var route = $"/actions/{actionName}";
        if (interceptor.BeforeGetInstancesEndpointDefinition(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = action.MapMethods(route, [method], async (IStateflowsStorage storage, ITenantAccessor tenantAccessor) =>
            {
                BehaviorClass[] actionClasses = [new ActionClass(actionName)];
                tenantAccessor.CurrentTenantId ??= "host";
                var contextIds = await storage.GetAllContextIdsAsync(actionClasses);
                return Results.Ok(contextIds.Select(id => new { Id = id }));
            })
            .WithTags($"{BehaviorType.Action} {actionName}");

            interceptor.AfterGetInstancesEndpointDefinition(behaviorClass, method, route, routeHandlerBuilder);
        }

        route = $"/actions/{actionName}/{{instance}}/status";
        method = HttpMethods.Get;
        if (interceptor.BeforeEventEndpointDefinition<BehaviorInfoRequest>(behaviorClass, isDefaultInstance: false, ref method, ref route))
        {
            var routeHandlerBuilder = action.MapMethods(
                route,
                [method],
                async (
                    string instance,
                    IActionLocator locator,
                    HttpContext httpContext,
                    [FromQuery] bool implicitInitialization = false,
                    [FromQuery] bool stream = false
                ) =>
                {
                    if (locator.TryLocateAction(new ActionId(actionName, instance), out var behavior))
                    {
                        if (stream)
                        {
                            httpContext.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");

                            await using var watcher = await behavior.WatchAsync(
                                [Event<BehaviorInfo>.Name],
                                async eventHolder => await httpContext.WriteEventAsync(eventHolder)
                            );

                            while (!httpContext.RequestAborted.IsCancellationRequested)
                            {
                                await Task.Delay(1000);
                            }

                            return Results.Empty;
                        }
                        else
                        {
                            var requestResult = await behavior.GetStatusAsync(
                                implicitInitialization
                                    ? []
                                    : new Dictionary<string, EventHeader> { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } }
                            );
                            // workaround for return code 200 regardless behavior actual status
                            requestResult.Status = EventStatus.Consumed;
                            return requestResult.ToResult([], requestResult.Response, HateoasLinks);
                        }
                    }

                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.Action} {actionName}");

            interceptor.AfterEventEndpointDefinition<BehaviorInfoRequest>(behaviorClass, isDefaultInstance: false, method, route, routeHandlerBuilder);

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

        route = $"/actions/{actionName}/{{instance}}/notifications";
        method = HttpMethods.Get;
        if (interceptor.BeforeEventEndpointDefinition<NotificationsRequest>(behaviorClass, isDefaultInstance: false, ref method, ref route))
        {
            var routeHandlerBuilder = action.MapMethods(
                route,
                [method],
                async (
                    string instance,
                    IActionLocator locator,
                    HttpContext httpContext,
                    [FromQuery] string[] names,
                    [FromQuery] TimeSpan? period,
                    [FromQuery] bool stream = false
                ) =>
                {
                    if (locator.TryLocateAction(new ActionId(actionName, instance), out var behavior))
                    {
                        if (stream)
                        {
                            period ??= TimeSpan.FromSeconds(0);

                            httpContext.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");

                            await using var watcher = await behavior.WatchAsync(
                                names,
                                async eventHolder => await httpContext.WriteEventAsync(eventHolder),
                                DateTime.Now - period.Value
                            );

                            while (!httpContext.RequestAborted.IsCancellationRequested)
                            {
                                await Task.Delay(1000);
                            }

                            return Results.Empty;
                        }
                        else
                        {
                            period ??= TimeSpan.FromSeconds(60);
                            var notifications = (await behavior.GetNotificationsAsync(names, DateTime.Now - period))
                                .ToArray();
                            var behaviorInfo = (await behavior.GetStatusAsync(new Dictionary<string, EventHeader>() { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } }))
                                .Response;
                            var sendResult = new SendResult(EventStatus.Consumed, new EventValidation(true));
                            return sendResult.ToResult(notifications, behaviorInfo, HateoasLinks);
                        }
                    }
                    return Results.NotFound();
                })
                .WithTags($"{BehaviorType.Action} {actionName}");

            interceptor.AfterEventEndpointDefinition<NotificationsRequest>(behaviorClass, isDefaultInstance: false, method, route, routeHandlerBuilder);

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

        route = $"/actions/{actionName}/{{instance}}/finalize";
        method = HttpMethods.Post;
        if (interceptor.BeforeEventEndpointDefinition<Finalize>(behaviorClass, isDefaultInstance: false, ref method, ref route))
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
                        var sendResult = await behavior.FinalizeAsync();
                        var behaviorInfo = (await behavior.GetStatusAsync(new Dictionary<string, EventHeader>() { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } })).Response;
                        return sendResult.ToResult([], behaviorInfo, HateoasLinks);
                    }

                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.Action} {actionName}");

            interceptor.AfterEventEndpointDefinition<Finalize>(behaviorClass, isDefaultInstance: false, method, route, routeHandlerBuilder);

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

        route = $"/actions/{actionName}/{{instance}}";
        method = HttpMethods.Delete;
        if (interceptor.BeforeEventEndpointDefinition<Reset>(behaviorClass, isDefaultInstance: false, ref method, ref route))
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
                        var sendResult = await behavior.ResetAsync();
                        var behaviorInfo = (await behavior.GetStatusAsync(new Dictionary<string, EventHeader>() { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } })).Response;
                        return sendResult.ToResult([], behaviorInfo, HateoasLinks);
                    }

                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.Action} {actionName}");

            interceptor.AfterEventEndpointDefinition<Reset>(behaviorClass, isDefaultInstance: false, method, route, routeHandlerBuilder);

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

        route = $"/actions/{actionName}/{{instance}}/initialize";
        method = HttpMethods.Post;
        if (interceptor.BeforeEventEndpointDefinition<Initialize>(behaviorClass, isDefaultInstance: false, ref method, ref route))
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
                            var sendResult = await behavior.SendAsync(new Initialize());
                            var behaviorInfo = (await behavior.GetStatusAsync(new Dictionary<string, EventHeader>() { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } })).Response;
                            return sendResult.ToResult([], behaviorInfo, HateoasLinks);
                        }

                        return Results.NotFound();
                    }
                )
                .WithTags($"{BehaviorType.Action} {actionName}");

            interceptor.AfterEventEndpointDefinition<Initialize>(behaviorClass, isDefaultInstance: false, method, route, routeHandlerBuilder);

            HateoasLinks.AddLink(
                behaviorClass.Name,
                new HateoasLink()
                {
                    Rel = "initialize",
                    Href = route,
                    Method = method
                },
                [BehaviorStatus.NotInitialized, BehaviorStatus.Unknown]
            );
        }
    }

    private void RegisterDefaultInstanceEndpoints(string actionName, IEndpointRouteBuilder action)
    {
        var behaviorClass = new ActionClass(actionName);
        const string instance = "";

        var method = HttpMethods.Get;
        var route = $"/actions/{actionName}";
        if (interceptor.BeforeGetInstancesEndpointDefinition(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = action.MapMethods(route, [method], async (IStateflowsStorage storage) =>
            {
                BehaviorClass[] actionClasses = [new ActionClass(actionName)];
                var contextIds = await storage.GetAllContextIdsAsync(actionClasses);
                return Results.Ok(contextIds.Select(id => new { Id = id }));
            })
            .WithTags($"{BehaviorType.Action} {actionName}");

            interceptor.AfterGetInstancesEndpointDefinition(behaviorClass, method, route, routeHandlerBuilder);
        }

        route = $"/actions/{actionName}/status";
        method = HttpMethods.Get;
        if (interceptor.BeforeEventEndpointDefinition<BehaviorInfoRequest>(behaviorClass, isDefaultInstance: true, ref method, ref route))
        {
            var routeHandlerBuilder = action.MapMethods(
                route,
                [method],
                async (
                    IActionLocator locator,
                    HttpContext httpContext,
                    [FromQuery] bool implicitInitialization = false,
                    [FromQuery] bool stream = false
                ) =>
                {
                    if (locator.TryLocateAction(new ActionId(actionName, instance), out var behavior))
                    {
                        if (stream)
                        {
                            httpContext.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");

                            await using var watcher = await behavior.WatchAsync(
                                [Event<BehaviorInfo>.Name],
                                async eventHolder => await httpContext.WriteEventAsync(eventHolder)
                            );

                            while (!httpContext.RequestAborted.IsCancellationRequested)
                            {
                                await Task.Delay(1000);
                            }

                            return Results.Empty;
                        }
                        else
                        {
                            var requestResult = await behavior.GetStatusAsync(
                                implicitInitialization
                                    ? []
                                    : new Dictionary<string, EventHeader> { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } }
                            );
                            // workaround for return code 200 regardless behavior actual status
                            requestResult.Status = EventStatus.Consumed;
                            return requestResult.ToResult([], requestResult.Response, HateoasLinks);
                        }
                    }

                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.Action} {actionName}");

            interceptor.AfterEventEndpointDefinition<BehaviorInfoRequest>(behaviorClass, isDefaultInstance: true, method, route, routeHandlerBuilder);

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

        route = $"/actions/{actionName}/notifications";
        method = HttpMethods.Get;
        if (interceptor.BeforeEventEndpointDefinition<NotificationsRequest>(behaviorClass, isDefaultInstance: true, ref method, ref route))
        {
            var routeHandlerBuilder = action.MapMethods(
                route,
                [method],
                async (
                    IActionLocator locator,
                    HttpContext httpContext,
                    [FromQuery] string[] names,
                    [FromQuery] TimeSpan? period,
                    [FromQuery] bool stream = false
                ) =>
                {
                    if (locator.TryLocateAction(new ActionId(actionName, instance), out var behavior))
                    {
                        if (stream)
                        {
                            period ??= TimeSpan.FromSeconds(0);

                            httpContext.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");

                            await using var watcher = await behavior.WatchAsync(
                                names,
                                async eventHolder => await httpContext.WriteEventAsync(eventHolder),
                                DateTime.Now - period.Value
                            );

                            while (!httpContext.RequestAborted.IsCancellationRequested)
                            {
                                await Task.Delay(1000);
                            }

                            return Results.Empty;
                        }
                        else
                        {
                            period ??= TimeSpan.FromSeconds(60);
                            var notifications = (await behavior.GetNotificationsAsync(names, DateTime.Now - period))
                                .ToArray();
                            var behaviorInfo = (await behavior.GetStatusAsync(new Dictionary<string, EventHeader>() { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } }))
                                .Response;
                            var sendResult = new SendResult(EventStatus.Consumed, new EventValidation(true));
                            return sendResult.ToResult(notifications, behaviorInfo, HateoasLinks);
                        }
                    }
                    return Results.NotFound();
                })
                .WithTags($"{BehaviorType.Action} {actionName}");

            interceptor.AfterEventEndpointDefinition<NotificationsRequest>(behaviorClass, isDefaultInstance: true, method, route, routeHandlerBuilder);

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
    }

    public override Task ActionTypeAddedAsync<TAction>(string actionName, int actionVersion)
    {
        if (OwnerClass != null)
        {
            return Task.CompletedTask;
        }

        var actionType = typeof(TAction);
        if (typeof(IActionEndpoints).IsAssignableFrom(actionType))
        {
            var endpointsBuilder = new EndpointsBuilder(routeBuilder, this, interceptor, new ActionClass(actionName), HasDefaultInstance);

            actionType.CallStaticMethod(nameof(IActionEndpoints.RegisterEndpoints), [typeof(IEndpointsBuilder)], [endpointsBuilder]);
        }

        return Task.CompletedTask;
    }
}