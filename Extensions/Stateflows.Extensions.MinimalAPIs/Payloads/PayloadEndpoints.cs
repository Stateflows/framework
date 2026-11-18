using System.ComponentModel.DataAnnotations;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Extensions.MinimalAPIs.Headers;

namespace Stateflows.Extensions.MinimalAPIs;

internal static class PayloadEndpoints
{
    private static readonly MethodInfo RequestMethod = typeof(PayloadEndpoints).GetMethod(
        nameof(RequestAsync),
        BindingFlags.Static | BindingFlags.NonPublic
    )!;

    public static void RegisterPayloadEndpoint(
        this IEndpointRouteBuilder routeBuilder,
        Interceptor interceptor,
        string behaviorType,
        string behaviorName,
        PayloadDefinition definition,
        Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> customHateoasLinks,
        bool hasDefaultInstance = false,
        BehaviorStatus[]? supportedStatuses = null,
        IAuthorizeData[]? authorization = null
    )
    {
        supportedStatuses ??= [BehaviorStatus.Initialized];
        var eventName = definition.Name.ToShortName().ToCamelCase();
        var behaviorClass = new BehaviorClass(behaviorType, behaviorName);

        var method = HttpMethods.Post;
        var route = $"/{behaviorType.ToResource()}/{behaviorName}/{{instance}}/{eventName}";
        if (interceptor.BeforePayloadEndpointDefinition(behaviorClass, definition, isDefaultInstance: false, ref method, ref route))
        {
            var endpointMethod = method;
            var endpointRoute = route;
            EndpointRouteRegistry.EnsureUnique(routeBuilder, method, route, definition.Name, isPayload: true);
            var routeHandlerBuilder = routeBuilder.MapMethods(
                route,
                [method],
                (
                    HttpContext context,
                    string instance,
                    [FromQuery] bool implicitInitialization = true,
                    [FromQuery] bool stream = false
                ) => HandleAsync(context, new BehaviorId(behaviorType, behaviorName, instance), definition,
                    endpointMethod, endpointRoute, implicitInitialization, stream, customHateoasLinks)
            );

            ConfigureEndpoint(routeHandlerBuilder, behaviorClass, definition, interceptor.ValidationSchemaMappers);
            if (authorization is { Length: > 0 })
                routeHandlerBuilder.RequireAuthorization(authorization);

            interceptor.AfterPayloadEndpointDefinition(behaviorClass, definition, isDefaultInstance: false, method, route, routeHandlerBuilder);

            customHateoasLinks.AddLink(
                behaviorClass.Name,
                new HateoasLink()
                {
                    Rel = eventName,
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

        method = HttpMethods.Post;
        var defaultInstanceRoute = $"/{behaviorType.ToResource()}/{behaviorName}/{eventName}";
        if (interceptor.BeforePayloadEndpointDefinition(behaviorClass, definition, isDefaultInstance: true, ref method, ref defaultInstanceRoute))
        {
            var endpointMethod = method;
            var endpointRoute = defaultInstanceRoute;
            EndpointRouteRegistry.EnsureUnique(routeBuilder, method, defaultInstanceRoute, definition.Name, isPayload: true);
            var routeHandlerBuilder = routeBuilder.MapMethods(
                defaultInstanceRoute,
                [method],
                (
                    HttpContext context,
                    [FromQuery] bool stream = false
                ) => HandleAsync(context, new BehaviorId(behaviorType, behaviorName, string.Empty), definition,
                    endpointMethod, endpointRoute, true, stream, customHateoasLinks)
            );

            ConfigureEndpoint(routeHandlerBuilder, behaviorClass, definition, interceptor.ValidationSchemaMappers);
            if (authorization is { Length: > 0 })
                routeHandlerBuilder.RequireAuthorization(authorization);

            interceptor.AfterPayloadEndpointDefinition(behaviorClass, definition, isDefaultInstance: true, method, defaultInstanceRoute, routeHandlerBuilder);

            customHateoasLinks.AddLink(
                behaviorClass.Name,
                new HateoasLink()
                {
                    Rel = eventName,
                    Href = defaultInstanceRoute,
                    Method = method
                },
                [BehaviorStatus.Initialized],
                eventName,
                "default:event"
            );
        }
    }

    private static void ConfigureEndpoint(RouteHandlerBuilder routeHandlerBuilder, BehaviorClass behaviorClass, PayloadDefinition definition,
        IReadOnlyDictionary<Type, Action<ValidationAttribute, Microsoft.OpenApi.OpenApiSchema>> validationSchemaMappers)
    {
        routeHandlerBuilder
            .WithTags($"{behaviorClass.Type} {behaviorClass.Name}")
            .WithMetadata(definition)
            .AddOpenApiOperationTransformer((operation, context, cancellationToken)
                => new PayloadOpenApi(validationSchemaMappers).ApplyAsync(operation, context, definition, cancellationToken));
    }

    private static async Task<IResult> HandleAsync(
        HttpContext context,
        BehaviorId behaviorId,
        PayloadDefinition definition,
        string method,
        string route,
        bool implicitInitialization,
        bool stream,
        Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> customHateoasLinks
    )
    {
        var services = context.RequestServices;
        var locator = services.GetRequiredService<IBehaviorLocator>();
        if (!locator.TryLocateBehavior(behaviorId, out var behavior))
        {
            return Results.NotFound();
        }

        var payload = await PayloadRequestReader.ReadAsync(context.Request, definition,
            services.GetRequiredService<IPayloadFactory>(), services.GetRequiredService<IPayloadsRegister>(), context.RequestAborted);
        var requestBody = new RequestBody { RequestedNotifications = payload.RequestedNotifications };

        var @event = payload.Event;
        var errors = payload.Errors;
        if (@event == null)
        {
            if (errors.Count == 0)
            {
                errors.Add(new ValidationResult("Event not provided"));
            }

            var result = new SendResult(EventStatus.Invalid, new EventValidation(false, errors));
            var behaviorInfo = await behavior.GetBehaviorInfo();
            return result.ToResult(method, route, [], behaviorInfo, customHateoasLinks);
        }

        Func<Task<IResult>> processingAction = () => ProcessAsync(context, behavior, definition, @event, requestBody, method, route, implicitInitialization, customHateoasLinks);

        if (stream)
        {
            await RequestBodyExtensions.SSENotificationsWatchAsync(context, behavior, requestBody, processingAction);

            return Results.Empty;
        }

        return await processingAction();
    }

    private static async Task<IResult> ProcessAsync(
        HttpContext context,
        IBehavior behavior,
        PayloadDefinition definition,
        Payload @event,
        RequestBody requestBody,
        string method,
        string route,
        bool implicitInitialization,
        Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> customHateoasLinks
    )
    {
        var headers = implicitInitialization
            ? new Dictionary<string, EventHeader>
            {
                { nameof(HttpContextHeader), new HttpContextHeader() { Context = context } }
            }
            : new Dictionary<string, EventHeader>
            {
                { nameof(NoImplicitInitialization), new NoImplicitInitialization() },
                { nameof(HttpContextHeader), new HttpContextHeader() { Context = context } }
            };

        var notifications = new ConcurrentQueue<EventHolder>();
        var notificationNames = requestBody.RequestedNotifications?.Where(n => !string.IsNullOrEmpty(n)).ToArray() ?? [];
        var watcher = notificationNames.Length > 0
            ? await behavior.WatchAsync(notificationNames, notifications.Enqueue)
            : null;

        try
        {
            if (!definition.IsRequest)
            {
                var result = await behavior.SendAsync(@event, headers);
                var behaviorInfo = await behavior.GetBehaviorInfo();
                if (watcher != null)
                    await watcher.WaitForDeliveryAsync().WaitAsync(context.RequestAborted);
                return result.ToResult(method, route, notifications.ToArray(), behaviorInfo, customHateoasLinks);
            }

            // Response contract is taken from the created request (metadata used by core dispatch), not inferred from the CLR type
            if (@event is not PayloadRequest payloadRequest)
            {
                throw new InvalidOperationException($"Payload '{definition.Name}' is declared as a request, but factory did not create a request.");
            }

            var responseType = payloadRequest.ResponseType ?? typeof(Payload);
            return await (Task<IResult>)RequestMethod
                .MakeGenericMethod(responseType)
                .Invoke(null, [behavior, @event, headers, method, route, notifications, watcher, context.RequestAborted, customHateoasLinks])!;
        }
        finally
        {
            if (watcher != null)
            {
                await watcher.DisposeAsync();
            }
        }
    }

    private static async Task<IResult> RequestAsync<TResponse>(
        IBehavior behavior,
        Payload request,
        Dictionary<string, EventHeader> headers,
        string method,
        string route,
        ConcurrentQueue<EventHolder> notifications,
        IWatcher? watcher,
        CancellationToken cancellationToken,
        Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> customHateoasLinks
    )
    {
        var result = await PayloadExtensions.RequestAsync<TResponse>(behavior, request, headers);
        var behaviorInfo = await behavior.GetBehaviorInfo();
        if (watcher != null)
            await watcher.WaitForDeliveryAsync().WaitAsync(cancellationToken);
        return result.ToResult(method, route, notifications.ToArray(), behaviorInfo, customHateoasLinks);
    }
}
