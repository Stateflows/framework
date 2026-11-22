using System.Threading.Channels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Extensions.MinimalAPIs.Attributes;
using Stateflows.Extensions.MinimalAPIs.Headers;

namespace Stateflows.Extensions.MinimalAPIs;

internal static class ActivityPinEndpoints
{
    internal static bool IsMappable<TToken>()
        => !typeof(TToken).IsDefined(typeof(NoApiMappingAttribute), inherit: true);

    internal static void RegisterPayloadPinEndpoint(this IEndpointRouteBuilder routes, Interceptor interceptor,
        PayloadDefinition definition, string activityName, bool input, bool hasDefaultInstance,
        Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> links)
    {
        var segment = definition.Name.ToShortName().ToCamelCase();
        foreach (var isDefault in hasDefaultInstance ? new[] { false, true } : new[] { false })
        {
            var method = input ? HttpMethods.Post : HttpMethods.Get;
            var direction = input ? "input" : "output";
            var route = $"/activities/{activityName}/{(isDefault ? "" : "{instance}/")}{direction}/{segment}";
            var behaviorClass = new BehaviorClass(BehaviorType.Activity, activityName);
            if (!interceptor.BeforePayloadEndpointDefinition(behaviorClass, definition, isDefault, ref method, ref route))
                continue;

            EndpointRouteRegistry.EnsureUnique(routes, method, route, definition.Name, isPayload: true);
            var endpointMethod = method;
            var endpointRoute = route;
            var endpoint = input
                ? isDefault
                    ? routes.MapMethods(route, [method], (
                        HttpContext context, IBehaviorLocator locator,
                        [FromQuery] bool implicitInitialization = true, [FromQuery] bool sendAsStream = false)
                        => SendPayloadInputAsync(context, locator, activityName, string.Empty, definition,
                            implicitInitialization, sendAsStream, endpointMethod, endpointRoute, links))
                    : routes.MapMethods(route, [method], (
                        HttpContext context, IBehaviorLocator locator, string instance,
                        [FromQuery] bool implicitInitialization = true, [FromQuery] bool sendAsStream = false)
                        => SendPayloadInputAsync(context, locator, activityName, instance, definition,
                            implicitInitialization, sendAsStream, endpointMethod, endpointRoute, links))
                : isDefault
                    ? routes.MapMethods(route, [method], (
                        HttpContext context, IBehaviorLocator locator, [FromQuery] bool stream = false)
                        => WatchPayloadOutputAsync(context, locator, activityName, string.Empty, definition, stream))
                    : routes.MapMethods(route, [method], (
                        HttpContext context, IBehaviorLocator locator, string instance, [FromQuery] bool stream = false)
                        => WatchPayloadOutputAsync(context, locator, activityName, instance, definition, stream));

            endpoint.WithTags($"{BehaviorType.Activity} {activityName}");
            if (input)
                PayloadEndpoints.ConfigureEndpoint(endpoint, behaviorClass, definition, interceptor.ValidationSchemaMappers);
            else
                endpoint.WithMetadata(definition)
                    .AddOpenApiOperationTransformer((operation, context, cancellationToken)
                        => new PayloadOpenApi(interceptor.ValidationSchemaMappers)
                            .ApplyOutputPinAsync(operation, context, definition, cancellationToken));
            interceptor.AfterPayloadEndpointDefinition(behaviorClass, definition, isDefault, method, route, endpoint);
            links.AddLink(activityName, new HateoasLink
            {
                Rel = $"{direction}/{segment}",
                Href = route,
                Method = method
            }, input
                ? [BehaviorStatus.NotInitialized, BehaviorStatus.Initialized]
                : [BehaviorStatus.NotInitialized, BehaviorStatus.Initialized, BehaviorStatus.Finalized]);
        }
    }

    internal static void RegisterInputPinEndpoint<TToken>(this IEndpointRouteBuilder routes, Interceptor interceptor,
        string activityName, bool hasDefaultInstance, Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> links)
    {
        foreach (var isDefault in hasDefaultInstance ? new[] { false, true } : new[] { false })
        {
            var method = HttpMethods.Post;
            var segment = Utils.GetEventName<TToken>();
            var route = $"/activities/{activityName}/{(isDefault ? "" : "{instance}/")}input/{segment}";
            var behaviorClass = new BehaviorClass(BehaviorType.Activity, activityName);
            if (!interceptor.BeforeEventEndpointDefinition<TokensInput<TToken>>(behaviorClass, isDefault, ref method, ref route))
                continue;

            EndpointRouteRegistry.EnsureUnique(routes, method, route, Event<TokensInput<TToken>>.Name, isPayload: true);
            var endpointMethod = method;
            var endpointRoute = route;
            var endpoint = isDefault
                ? routes.MapMethods(route, [method], (
                    HttpContext context, IBehaviorLocator locator, [FromBody] ActivityInputBody<TToken> payload,
                    [FromQuery] bool implicitInitialization = true, [FromQuery] bool sendAsStream = false)
                    => SendInputAsync(context, locator, activityName, string.Empty, payload,
                        implicitInitialization, sendAsStream, endpointMethod, endpointRoute, links))
                : routes.MapMethods(route, [method], (
                    HttpContext context, IBehaviorLocator locator, string instance, [FromBody] ActivityInputBody<TToken> payload,
                    [FromQuery] bool implicitInitialization = true, [FromQuery] bool sendAsStream = false)
                    => SendInputAsync(context, locator, activityName, instance, payload,
                        implicitInitialization, sendAsStream, endpointMethod, endpointRoute, links));

            endpoint.WithTags($"{BehaviorType.Activity} {activityName}");
            interceptor.AfterEventEndpointDefinition<TokensInput<TToken>>(behaviorClass, isDefault, method, route, endpoint);
            links.AddLink(activityName, new HateoasLink { Rel = $"input/{segment}", Href = route, Method = method },
                [BehaviorStatus.NotInitialized, BehaviorStatus.Initialized]);
        }
    }

    private static async Task<IResult> SendInputAsync<TToken>(HttpContext context, IBehaviorLocator locator,
        string activityName, string instance, ActivityInputBody<TToken> payload, bool implicitInitialization,
        bool sendAsStream, string method, string route, Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> links)
    {
        var (authorized, failure) = await Utils.AuthorizeEventAsync(typeof(TToken), context.RequestServices, context);
        if (!authorized) return failure!;
        if (payload.Tokens == null)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["tokens"] = ["A token batch is required."] });
        if (!locator.TryLocateBehavior(new BehaviorId(BehaviorType.Activity, activityName, instance), out var behavior))
            return Results.NotFound();
        using (behavior)
        {
            var headers = new Dictionary<string, EventHeader>
            {
                [nameof(HttpContextHeader)] = new HttpContextHeader { Context = context }
            };
            if (!implicitInitialization) headers[nameof(NoImplicitInitialization)] = new NoImplicitInitialization();
            if (sendAsStream) headers[nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream();
            var result = await behavior.SendAsync(new TokensInput<TToken>().AddRange(payload.Tokens), headers);
            // Admission must not be followed by a status request that queues behind regular work.
            if (sendAsStream)
                return PayloadJson.SerializeObject(new ResponseBody(result, [], [], new Dictionary<string, object>
                {
                    ["id"] = behavior.Id
                })).ToResult(result.Status);
            return result.ToResult(method, route, [], await behavior.GetBehaviorInfo(), links);
        }
    }

    private static async Task<IResult> SendPayloadInputAsync(HttpContext context, IBehaviorLocator locator,
        string activityName, string instance, PayloadDefinition definition, bool implicitInitialization,
        bool sendAsStream, string method, string route, Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> links)
    {
        var services = context.RequestServices;
        var payload = await PayloadRequestReader.ReadAsync(context.Request, definition,
            services.GetRequiredService<IPayloadFactory>(), services.GetRequiredService<IPayloadsRegister>(), context.RequestAborted);
        if (payload.Event == null)
        {
            var errors = payload.Errors
                .GroupBy(error => error.MemberNames.FirstOrDefault() ?? "event")
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage ?? "Invalid payload.").ToArray());
            return Results.ValidationProblem(errors);
        }

        if (!locator.TryLocateBehavior(new BehaviorId(BehaviorType.Activity, activityName, instance), out var behavior))
            return Results.NotFound();

        using (behavior)
        {
            var headers = new Dictionary<string, EventHeader>
            {
                [nameof(HttpContextHeader)] = new HttpContextHeader { Context = context }
            };
            if (!implicitInitialization)
                headers[nameof(NoImplicitInitialization)] = new NoImplicitInitialization();
            if (sendAsStream)
                headers[nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream();

            var result = await behavior.SendAsync(new TokensInput().Add(payload.Event), headers);
            if (sendAsStream)
                return PayloadJson.SerializeObject(new ResponseBody(result, [], [], new Dictionary<string, object>
                {
                    ["id"] = behavior.Id
                })).ToResult(result.Status);
            return result.ToResult(method, route, [], await behavior.GetBehaviorInfo(), links);
        }
    }

    internal static void RegisterOutputPinEndpoint<TToken>(this IEndpointRouteBuilder routes, Interceptor interceptor,
        string activityName, bool hasDefaultInstance, Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> links)
    {
        foreach (var isDefault in hasDefaultInstance ? new[] { false, true } : new[] { false })
        {
            var method = HttpMethods.Get;
            var segment = Utils.GetEventName<TToken>();
            var route = $"/activities/{activityName}/{(isDefault ? "" : "{instance}/")}output/{segment}";
            var behaviorClass = new BehaviorClass(BehaviorType.Activity, activityName);
            if (!interceptor.BeforeEventEndpointDefinition<TToken>(behaviorClass, isDefault, ref method, ref route))
                continue;

            EndpointRouteRegistry.EnsureUnique(routes, method, route, Event<TToken>.Name, isPayload: true);
            var endpoint = isDefault
                ? routes.MapMethods(route, [method], (
                    HttpContext context, IBehaviorLocator locator, [FromQuery] bool stream = false)
                    => WatchOutputAsync<TToken>(context, locator, activityName, string.Empty, stream))
                : routes.MapMethods(route, [method], (
                    HttpContext context, IBehaviorLocator locator, string instance, [FromQuery] bool stream = false)
                    => WatchOutputAsync<TToken>(context, locator, activityName, instance, stream));
            endpoint.WithTags($"{BehaviorType.Activity} {activityName}");
            interceptor.AfterEventEndpointDefinition<TToken>(behaviorClass, isDefault, method, route, endpoint);
            links.AddLink(activityName, new HateoasLink { Rel = $"output/{segment}", Href = route, Method = method },
                [BehaviorStatus.NotInitialized, BehaviorStatus.Initialized, BehaviorStatus.Finalized]);
        }
    }

    private static async Task<IResult> WatchOutputAsync<TToken>(HttpContext context, IBehaviorLocator locator,
        string activityName, string instance, bool stream)
    {
        var (authorized, failure) = await Utils.AuthorizeEventAsync(typeof(TToken), context.RequestServices, context);
        if (!authorized) return failure!;
        if (!locator.TryLocateBehavior(new BehaviorId(BehaviorType.Activity, activityName, instance), out var behavior))
            return Results.NotFound();
        using (behavior)
        {
            var watchFrom = DateTime.Now;
            if (!stream)
            {
                var next = new TaskCompletionSource<TToken>(TaskCreationOptions.RunContinuationsAsynchronously);
                await using var watcher = await behavior.WatchAsync([Event<TToken>.Name], notification =>
                {
                    if (notification.SentAt >= watchFrom)
                        next.TrySetResult(((EventHolder<TToken>)notification).Payload);
                }, watchFrom);
                try
                {
                    var notification = await next.Task.WaitAsync(context.RequestAborted);
                    return Results.Json(notification);
                }
                catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
                {
                    return Results.Empty;
                }
            }

            // Do not perform HTTP writes on the publisher's execution path. Disconnect slow consumers explicitly.
            var queue = Channel.CreateBounded<EventHolder>(new BoundedChannelOptions(128)
            {
                SingleReader = true,
                FullMode = BoundedChannelFullMode.Wait
            });
            await using var outputWatcher = await behavior.WatchAsync([Event<TToken>.Name], notification =>
            {
                if (notification.SentAt < watchFrom) return;
                if (!queue.Writer.TryWrite(notification))
                    queue.Writer.TryComplete(new InvalidOperationException("Activity output SSE consumer exceeded its notification buffer."));
            }, watchFrom);
            context.Response.ContentType = "text/event-stream";
            context.Response.Headers.CacheControl = "no-cache";
            await context.Response.StartAsync(context.RequestAborted);
            await context.Response.Body.FlushAsync(context.RequestAborted);
            try
            {
                await foreach (var notification in queue.Reader.ReadAllAsync(context.RequestAborted))
                {
                    await context.Response.WriteAsync($"event: {notification.Name}\ndata: {PayloadJson.SerializeObject(notification)}\n\n",
                        context.RequestAborted);
                    await context.Response.Body.FlushAsync(context.RequestAborted);
                }
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
            }
            catch (Exception exception) when (exception is IOException or InvalidOperationException)
            {
                context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Stateflows.ActivityOutput")
                    .LogError(exception, "Activity output SSE connection failed for {BehaviorId}.", behavior.Id);
                context.Abort();
            }
            finally
            {
                queue.Writer.TryComplete();
            }
            return Results.Empty;
        }
    }

    private static async Task<IResult> WatchPayloadOutputAsync(HttpContext context, IBehaviorLocator locator,
        string activityName, string instance, PayloadDefinition definition, bool stream)
    {
        if (!locator.TryLocateBehavior(new BehaviorId(BehaviorType.Activity, activityName, instance), out var behavior))
            return Results.NotFound();
        using (behavior)
        {
            var watchFrom = DateTime.Now;
            if (!stream)
            {
                var next = new TaskCompletionSource<Payload>(TaskCreationOptions.RunContinuationsAsynchronously);
                await using var watcher = await behavior.WatchAsync([definition.Name], notification =>
                {
                    if (notification.SentAt >= watchFrom &&
                        notification is EventHolder<Payload> holder &&
                        holder.Payload.Name == definition.Name)
                        next.TrySetResult(holder.Payload);
                }, watchFrom);
                try
                {
                    var payload = await next.Task.WaitAsync(context.RequestAborted);
                    return Results.Content(PayloadJson.SerializeObject(payload), "application/json");
                }
                catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
                {
                    return Results.Empty;
                }
            }

            var queue = Channel.CreateBounded<EventHolder>(new BoundedChannelOptions(128)
            {
                SingleReader = true,
                FullMode = BoundedChannelFullMode.Wait
            });
            await using var outputWatcher = await behavior.WatchAsync([definition.Name], notification =>
            {
                if (notification.SentAt < watchFrom ||
                    notification is not EventHolder<Payload> holder ||
                    holder.Payload.Name != definition.Name)
                    return;
                if (!queue.Writer.TryWrite(notification))
                    queue.Writer.TryComplete(new InvalidOperationException("Activity output SSE consumer exceeded its notification buffer."));
            }, watchFrom);
            context.Response.ContentType = "text/event-stream";
            context.Response.Headers.CacheControl = "no-cache";
            await context.Response.StartAsync(context.RequestAborted);
            await context.Response.Body.FlushAsync(context.RequestAborted);
            try
            {
                await foreach (var notification in queue.Reader.ReadAllAsync(context.RequestAborted))
                {
                    await context.Response.WriteAsync($"event: {notification.Name}\ndata: {PayloadJson.SerializeObject(notification)}\n\n",
                        context.RequestAborted);
                    await context.Response.Body.FlushAsync(context.RequestAborted);
                }
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
            }
            catch (Exception exception) when (exception is IOException or InvalidOperationException)
            {
                context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Stateflows.ActivityOutput")
                    .LogError(exception, "Activity payload output SSE connection failed for {BehaviorId}.", behavior.Id);
                context.Abort();
            }
            finally
            {
                queue.Writer.TryComplete();
            }
            return Results.Empty;
        }
    }
}
