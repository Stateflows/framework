using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Entities;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;
using Stateflows.Extensions.MinimalAPIs.Attributes;
using Stateflows.Extensions.MinimalAPIs.Headers;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>
/// Maps Minimal API endpoints for registered Entities. Elements of each Entity are collected while visiting and mapped
/// by <see cref="RegisterEndpoints"/>, so that only the newest version of each Entity is exposed.
/// </summary>
internal class EntityVisitor(
    IEndpointRouteBuilder routeBuilder,
    Interceptor interceptor
) : Entities.EntityVisitor
{
    private sealed class EntityDefinition(string name, int version)
    {
        public string Name { get; } = name;
        public int Version { get; } = version;
        public Dictionary<Type, System.Action> Initializers { get; } = new();
        public Dictionary<Type, System.Action> Mutations { get; } = new();
        public Dictionary<Type, System.Action> Projections { get; } = new();
        public Dictionary<string, System.Action> Fields { get; } = new();
    }

    private readonly Dictionary<string, EntityDefinition> definitions = new();
    private EntityDefinition? current;

    public Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> HateoasLinks { get; } = new();

    // Runs after all configured interceptors, so it observes the final method and route of every entity endpoint.
    private readonly Interceptor entityInterceptor = new([interceptor, new RouteConflictDetector()]);

    public override Task EntityAddingAsync<TTemplate>(string entityName, int entityVersion,
        BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false)
    {
        current = ownerClass == null
            ? new EntityDefinition(entityName, entityVersion)
            : null;

        return Task.CompletedTask;
    }

    public override Task EntityAddedAsync<TTemplate>(string entityName, int entityVersion)
    {
        if (IsCurrent(entityName, entityVersion))
        {
            if (!definitions.TryGetValue(entityName, out var existing) || existing.Version < entityVersion)
            {
                definitions[entityName] = current!;
            }
        }

        current = null;

        return Task.CompletedTask;
    }

    public override Task InitializerAddedAsync<TTemplate, TInitializationEvent>(string entityName, int entityVersion)
    {
        if (IsCurrent(entityName, entityVersion))
        {
            current!.Initializers[typeof(TInitializationEvent)] = () =>
                RegisterEventEndpoint<TInitializationEvent>(entityName, [BehaviorStatus.NotInitialized]);
        }

        return Task.CompletedTask;
    }

    public override Task MutationAddedAsync<TTemplate, TMutationEvent>(string entityName, int entityVersion)
    {
        if (IsCurrent(entityName, entityVersion))
        {
            var methods = new[] { typeof(TTemplate) }.Concat(typeof(TTemplate).GetInterfaces())
                .SelectMany(type => type.GetMethods())
                .Where(method => method.IsDefined(typeof(MutationAttribute)) &&
                    method.GetParameters() is [var parameter] && parameter.ParameterType == typeof(TMutationEvent))
                .Distinct();
            var authorization = GetAccessorAuthorization(methods);
            current!.Mutations[typeof(TMutationEvent)] = () =>
                RegisterEventEndpoint<TMutationEvent>(entityName, [BehaviorStatus.Initialized], authorization);
        }

        return Task.CompletedTask;
    }

    public override Task ProjectionAddedAsync<TTemplate, TProjectionTemplate>(string entityName, int entityVersion, PublishScope publishScope)
    {
        if (IsCurrent(entityName, entityVersion))
        {
            var getters = new[] { typeof(TTemplate) }.Concat(typeof(TTemplate).GetInterfaces())
                .SelectMany(type => type.GetProperties())
                .Where(property => property.PropertyType == typeof(TProjectionTemplate) &&
                    property.IsDefined(typeof(ProjectionAttribute)))
                .Select(property => property.GetMethod)
                .Distinct();
            var authorization = GetAccessorAuthorization(getters);
            current!.Projections[typeof(TProjectionTemplate)] = () =>
                RegisterProjectionEndpoint<TProjectionTemplate>(entityName, publishScope.HasFlag(PublishScope.Self), authorization);
        }

        return Task.CompletedTask;
    }

    public override Task FieldAddedAsync<TTemplate, TField>(string entityName, int entityVersion, string fieldName,
        TField defaultValue, bool computed, FieldAccess access)
    {
        if (IsCurrent(entityName, entityVersion))
        {
            var properties = new[] { typeof(TTemplate) }.Concat(typeof(TTemplate).GetInterfaces())
                .SelectMany(type => type.GetProperties())
                .Where(property => property.Name == fieldName && property.PropertyType == typeof(TField))
                .Distinct()
                .ToArray();
            if (properties.Any(property => property.IsDefined(typeof(NoApiMappingAttribute), inherit: true)))
            {
                return Task.CompletedTask;
            }
            var mapGetter = !properties.Any(property =>
                property.GetMethod?.IsDefined(typeof(NoApiMappingAttribute), inherit: true) == true);
            var mapSetter = !properties.Any(property =>
                property.SetMethod?.IsDefined(typeof(NoApiMappingAttribute), inherit: true) == true);
            var getterAuthorization = GetAccessorAuthorization(properties.Select(property => property.GetMethod));
            var setterAuthorization = GetAccessorAuthorization(properties.Select(property => property.SetMethod));

            current!.Fields[fieldName] = () =>
            {
                if (mapGetter && access.HasFlag(FieldAccess.Get))
                {
                    RegisterFieldGetEndpoint<TField>(entityName, fieldName, getterAuthorization);
                }

                if (mapSetter && access.HasFlag(FieldAccess.Set))
                {
                    RegisterFieldSetEndpoint<TField>(entityName, fieldName, setterAuthorization);
                }
            };
        }

        return Task.CompletedTask;
    }

    private static IAuthorizeData[] GetAccessorAuthorization(IEnumerable<MethodInfo?> accessors)
        => accessors.OfType<MethodInfo>()
            .SelectMany(accessor => accessor.GetCustomAttributes(inherit: true).OfType<IAuthorizeData>())
            .ToArray();

    private bool IsCurrent(string entityName, int entityVersion)
        => current != null && current.Name == entityName && current.Version == entityVersion;

    public void RegisterEndpoints()
    {
        foreach (var definition in definitions.Values)
        {
            RegisterStandardEndpoints(definition.Name);

            if (definition.Initializers.Count == 0)
            {
                RegisterEventEndpoint<Initialize>(definition.Name, [BehaviorStatus.NotInitialized]);
            }

            foreach (var register in definition.Initializers.Values
                         .Concat(definition.Mutations.Values)
                         .Concat(definition.Projections.Values)
                         .Concat(definition.Fields.Values))
            {
                register();
            }
        }
    }

    private static string GetRoute(string entityName, string suffix = "")
        => $"/{BehaviorType.Entity.ToResource()}/{entityName}/{{instance}}{suffix}";

    private static Dictionary<string, EventHeader> GetHeaders(bool implicitInitialization, HttpContext httpContext)
        => implicitInitialization
            ? new Dictionary<string, EventHeader>
            {
                { nameof(HttpContextHeader), new HttpContextHeader() { Context = httpContext } }
            }
            : new Dictionary<string, EventHeader>
            {
                { nameof(NoImplicitInitialization), new NoImplicitInitialization() },
                { nameof(HttpContextHeader), new HttpContextHeader() { Context = httpContext } }
            };

    private static Dictionary<string, EventHeader> NoImplicitInitializationHeaders
        => new() { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } };

    private static bool IsMappable(Type eventType)
        => !(
            eventType.GetCustomAttributes<NoApiMappingAttribute>().Any() ||
            eventType.IsAssignableTo(typeof(SystemEvent)) ||
            eventType.IsAssignableTo(typeof(Exception))
        );

    private void RegisterEventEndpoint<TEvent>(string entityName, BehaviorStatus[] supportedStatuses,
        IAuthorizeData[]? authorization = null)
    {
        if (!IsMappable(typeof(TEvent)))
        {
            return;
        }

        routeBuilder.RegisterEventEndpoint<TEvent>(entityInterceptor, BehaviorType.Entity, entityName, HateoasLinks,
            hasDefaultInstance: false, supportedStatuses, authorization);
    }

    private void RegisterStandardEndpoints(string entityName)
    {
        var behaviorClass = new EntityClass(entityName).BehaviorClass;
        var tags = $"{BehaviorType.Entity} {entityName}";

        var method = HttpMethods.Get;
        var route = $"/{BehaviorType.Entity.ToResource()}/{entityName}";
        if (entityInterceptor.BeforeGetInstancesEndpointDefinition(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(route, [method], async (IStateflowsStorage storage, ITenantAccessor tenantAccessor) =>
                {
                    tenantAccessor.CurrentTenantId ??= "host";
                    var contextIds = await storage.GetAllContextIdsAsync([behaviorClass]);
                    return Results.Ok(contextIds.Select(id => new { Id = id }));
                })
                .WithTags(tags);

            entityInterceptor.AfterGetInstancesEndpointDefinition(behaviorClass, method, route, routeHandlerBuilder);
        }

        var statusMethod = HttpMethods.Get;
        var statusRoute = GetRoute(entityName, "/status");
        if (entityInterceptor.BeforeEventEndpointDefinition<EntityInfoRequest>(behaviorClass, isDefaultInstance: false, ref statusMethod, ref statusRoute))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(
                    statusRoute,
                    [statusMethod],
                    async (
                        string instance,
                        IBehaviorLocator locator,
                        HttpContext httpContext,
                        [FromQuery] bool implicitInitialization = false,
                        [FromQuery] bool stream = false
                    ) =>
                    {
                        if (!locator.TryLocateBehavior(new EntityClass(entityName).ToId(instance), out var behavior))
                        {
                            return Results.NotFound();
                        }

                        if (stream)
                        {
                            httpContext.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");

                            await using var watcher = await behavior.WatchAsync(
                                [Event<EntityInfo>.Name],
                                async eventHolder => await httpContext.WriteEventAsync(eventHolder)
                            );

                            await WaitForAbortAsync(httpContext);

                            return Results.Empty;
                        }

                        var requestResult = await behavior.RequestAsync(
                            new EntityInfoRequest(),
                            implicitInitialization ? new Dictionary<string, EventHeader>() : NoImplicitInitializationHeaders
                        );
                        // status endpoint returns 200 regardless of entity status
                        requestResult.Status = EventStatus.Consumed;
                        return requestResult.ToResult(statusMethod, statusRoute, [], requestResult.Response, HateoasLinks);
                    }
                )
                .WithTags(tags);

            entityInterceptor.AfterEventEndpointDefinition<EntityInfoRequest>(behaviorClass, isDefaultInstance: false, statusMethod, statusRoute, routeHandlerBuilder);

            HateoasLinks.AddLink(
                entityName,
                new HateoasLink() { Rel = "status", Href = statusRoute, Method = statusMethod },
                [BehaviorStatus.NotInitialized, BehaviorStatus.Initialized]
            );
        }

        var notificationsMethod = HttpMethods.Get;
        var notificationsRoute = GetRoute(entityName, "/notifications");
        if (entityInterceptor.BeforeEventEndpointDefinition<NotificationsRequest>(behaviorClass, isDefaultInstance: false, ref notificationsMethod, ref notificationsRoute))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(
                    notificationsRoute,
                    [notificationsMethod],
                    async (
                        string instance,
                        IBehaviorLocator locator,
                        HttpContext httpContext,
                        [FromQuery] string[] names,
                        [FromQuery] TimeSpan? period,
                        [FromQuery] bool stream = false
                    ) =>
                    {
                        if (!locator.TryLocateBehavior(new EntityClass(entityName).ToId(instance), out var behavior))
                        {
                            return Results.NotFound();
                        }

                        if (stream)
                        {
                            period ??= TimeSpan.FromSeconds(0);

                            httpContext.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");

                            await using var watcher = await behavior.WatchAsync(
                                names,
                                async eventHolder => await httpContext.WriteEventAsync(eventHolder),
                                DateTime.Now - period.Value
                            );

                            await WaitForAbortAsync(httpContext);

                            return Results.Empty;
                        }

                        period ??= TimeSpan.FromSeconds(60);
                        var notifications = (await behavior.GetNotificationsAsync(names, DateTime.Now - period)).ToArray();
                        var behaviorInfo = await behavior.GetBehaviorInfo();
                        var sendResult = new SendResult(EventStatus.Consumed, new EventValidation(true));
                        return sendResult.ToResult(notificationsMethod, notificationsRoute, notifications, behaviorInfo, HateoasLinks);
                    })
                .WithTags(tags);

            entityInterceptor.AfterEventEndpointDefinition<NotificationsRequest>(behaviorClass, isDefaultInstance: false, notificationsMethod, notificationsRoute, routeHandlerBuilder);

            HateoasLinks.AddLink(
                entityName,
                new HateoasLink() { Rel = "notifications", Href = notificationsRoute, Method = notificationsMethod },
                [BehaviorStatus.Initialized]
            );
        }

        var resetMethod = HttpMethods.Delete;
        var resetRoute = GetRoute(entityName);
        if (entityInterceptor.BeforeEventEndpointDefinition<Reset>(behaviorClass, isDefaultInstance: false, ref resetMethod, ref resetRoute))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(
                    resetRoute,
                    [resetMethod],
                    async (
                        string instance,
                        IBehaviorLocator locator
                    ) =>
                    {
                        if (!locator.TryLocateBehavior(new EntityClass(entityName).ToId(instance), out var behavior))
                        {
                            return Results.NotFound();
                        }

                        var sendResult = await behavior.ResetAsync();
                        var behaviorInfo = await behavior.GetBehaviorInfo();
                        return sendResult.ToResult(resetMethod, resetRoute, [], behaviorInfo, HateoasLinks);
                    }
                )
                .WithTags(tags);

            entityInterceptor.AfterEventEndpointDefinition<Reset>(behaviorClass, isDefaultInstance: false, resetMethod, resetRoute, routeHandlerBuilder);

            HateoasLinks.AddLink(
                entityName,
                new HateoasLink() { Rel = "reset", Href = resetRoute, Method = resetMethod },
                [BehaviorStatus.Initialized]
            );
        }
    }

    private void RegisterProjectionEndpoint<TProjection>(string entityName, bool streamable, IAuthorizeData[] authorization)
    {
        var projectionType = typeof(TProjection);
        if (!IsMappable(projectionType))
        {
            return;
        }

        var behaviorClass = new EntityClass(entityName).BehaviorClass;
        var method = HttpMethods.Get;
        var route = GetRoute(entityName, $"/{Utils.GetEventName<TProjection>()}");
        if (!entityInterceptor.BeforeEventEndpointDefinition<ProjectionRequest<TProjection>>(behaviorClass, isDefaultInstance: false, ref method, ref route))
        {
            return;
        }

        async Task<IResult> HandleAsync(HttpContext httpContext, IServiceProvider serviceProvider, string instance,
            IBehaviorLocator locator, bool implicitInitialization, bool stream)
        {
            var (success, authorizationResult) = await Utils.AuthorizeEventAsync(projectionType, serviceProvider, httpContext);
            if (!success)
            {
                return authorizationResult!;
            }

            if (!locator.TryLocateBehavior(new EntityClass(entityName).ToId(instance), out var behavior))
            {
                return Results.NotFound();
            }

            return stream
                ? await StreamProjectionAsync<TProjection>(httpContext, behavior, implicitInitialization, method, route, HateoasLinks)
                : await new RequestBody().RequestEndpointAsync<ProjectionRequest<TProjection>, TProjection>(method, route,
                    new ProjectionRequest<TProjection>(), behavior, implicitInitialization, HateoasLinks, httpContext);
        }

        // Only projections published to the entity itself can be watched, so only they accept the "stream" parameter.
        var routeHandlerBuilder = streamable
            ? routeBuilder.MapMethods(
                route,
                [method],
                (
                    HttpContext httpContext,
                    IServiceProvider serviceProvider,
                    string instance,
                    IBehaviorLocator locator,
                    [FromQuery] bool implicitInitialization = false,
                    [FromQuery] bool stream = false
                ) => HandleAsync(httpContext, serviceProvider, instance, locator, implicitInitialization, stream)
            )
            : routeBuilder.MapMethods(
                route,
                [method],
                (
                    HttpContext httpContext,
                    IServiceProvider serviceProvider,
                    string instance,
                    IBehaviorLocator locator,
                    [FromQuery] bool implicitInitialization = false
                ) => HandleAsync(httpContext, serviceProvider, instance, locator, implicitInitialization, false)
            );

        routeHandlerBuilder.WithTags($"{BehaviorType.Entity} {entityName}");
        if (authorization.Length > 0)
            routeHandlerBuilder.RequireAuthorization(authorization);

        entityInterceptor.AfterEventEndpointDefinition<ProjectionRequest<TProjection>>(behaviorClass, isDefaultInstance: false, method, route, routeHandlerBuilder);

        HateoasLinks.AddLink(
            entityName,
            new HateoasLink() { Rel = Utils.GetEventName<TProjection>(), Href = route, Method = method },
            [BehaviorStatus.Initialized]
        );
    }

    private static async Task<IResult> StreamProjectionAsync<TProjection>(HttpContext httpContext, IBehavior behavior,
        bool implicitInitialization, string method, string route, Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> hateoasLinks)
    {
        // Recalculations are held back until the current value is written, so the stream always starts with it.
        // A recalculation published just before the request is processed may therefore repeat the current value.
        var gate = new SemaphoreSlim(0, 1);
        await using var watcher = await behavior.WatchAsync(
            [Event<TProjection>.Name],
            async eventHolder =>
            {
                try
                {
                    await gate.WaitAsync(httpContext.RequestAborted);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                try
                {
                    await httpContext.WriteEventAsync(eventHolder);
                }
                finally
                {
                    gate.Release();
                }
            }
        );

        var requestResult = await behavior.RequestAsync(new ProjectionRequest<TProjection>(), GetHeaders(implicitInitialization, httpContext));
        if (requestResult.Status != EventStatus.Consumed)
        {
            var behaviorInfo = await behavior.GetBehaviorInfo();
            return requestResult.ToResult(method, route, [], behaviorInfo, hateoasLinks);
        }

        httpContext.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");
        await httpContext.WriteEventAsync(requestResult.Response.ToEventHolder(behavior.Id));
        gate.Release();

        await WaitForAbortAsync(httpContext);

        return Results.Empty;
    }

    private void RegisterFieldGetEndpoint<TField>(string entityName, string fieldName, IAuthorizeData[] authorization)
    {
        var behaviorClass = new EntityClass(entityName).BehaviorClass;
        var method = HttpMethods.Get;
        var route = GetRoute(entityName, $"/{fieldName.ToCamelCase()}");
        if (!entityInterceptor.BeforeEventEndpointDefinition<FieldStateRequest<TField>>(behaviorClass, isDefaultInstance: false, ref method, ref route))
        {
            return;
        }

        var routeHandlerBuilder = routeBuilder.MapMethods(
                route,
                [method],
                async (
                    HttpContext httpContext,
                    string instance,
                    IBehaviorLocator locator,
                    [FromQuery] bool implicitInitialization = false
                ) =>
                {
                    if (!locator.TryLocateBehavior(new EntityClass(entityName).ToId(instance), out var behavior))
                    {
                        return Results.NotFound();
                    }

                    return await new RequestBody().RequestEndpointAsync<FieldStateRequest<TField>, FieldState<TField>>(method, route,
                        new FieldStateRequest<TField>() { Name = fieldName }, behavior, implicitInitialization, HateoasLinks, httpContext);
                }
            )
            .WithTags($"{BehaviorType.Entity} {entityName}");

        if (authorization.Length > 0)
        {
            routeHandlerBuilder.RequireAuthorization(authorization);
        }

        entityInterceptor.AfterEventEndpointDefinition<FieldStateRequest<TField>>(behaviorClass, isDefaultInstance: false, method, route, routeHandlerBuilder);

        HateoasLinks.AddLink(
            entityName,
            new HateoasLink() { Rel = fieldName.ToCamelCase(), Href = route, Method = method },
            [BehaviorStatus.Initialized]
        );
    }

    private void RegisterFieldSetEndpoint<TField>(string entityName, string fieldName, IAuthorizeData[] authorization)
    {
        var behaviorClass = new EntityClass(entityName).BehaviorClass;
        var method = HttpMethods.Put;
        var route = GetRoute(entityName, $"/{fieldName.ToCamelCase()}");
        if (!entityInterceptor.BeforeEventEndpointDefinition<FieldState<TField>>(behaviorClass, isDefaultInstance: false, ref method, ref route))
        {
            return;
        }

        var routeHandlerBuilder = routeBuilder.MapMethods(
                route,
                [method],
                async (
                    HttpContext httpContext,
                    string instance,
                    IBehaviorLocator locator,
                    RequestBody<TField> payload,
                    [FromQuery] bool implicitInitialization = true
                ) =>
                {
                    if (!locator.TryLocateBehavior(new EntityClass(entityName).ToId(instance), out var behavior))
                    {
                        return Results.NotFound();
                    }

                    return await payload.SendEndpointAsync(method, route,
                        new FieldState<TField>() { Name = fieldName, Value = payload.Event }, behavior, implicitInitialization, HateoasLinks, httpContext);
                }
            )
            .WithTags($"{BehaviorType.Entity} {entityName}");

        if (authorization.Length > 0)
        {
            routeHandlerBuilder.RequireAuthorization(authorization);
        }

        entityInterceptor.AfterEventEndpointDefinition<FieldState<TField>>(behaviorClass, isDefaultInstance: false, method, route, routeHandlerBuilder);

        HateoasLinks.AddLink(
            entityName,
            new HateoasLink() { Rel = fieldName.ToCamelCase(), Href = route, Method = method },
            [BehaviorStatus.Initialized]
        );
    }

    private static async Task WaitForAbortAsync(HttpContext httpContext)
    {
        try
        {
            await Task.Delay(Timeout.Infinite, httpContext.RequestAborted);
        }
        catch (OperationCanceledException)
        { }
    }

    /// <summary>
    /// Fails endpoint mapping when two entity endpoints resolve to the same HTTP method and route
    /// (f.e. a field named "Status" and the standard status endpoint).
    /// </summary>
    private sealed class RouteConflictDetector : EndpointDefinitionInterceptor
    {
        private readonly Dictionary<string, string> routes = new(StringComparer.OrdinalIgnoreCase);

        private void Register(string method, string route, string description)
        {
            var key = $"{method} {route}";
            if (routes.TryGetValue(key, out var existing))
            {
                throw new InvalidOperationException(
                    $"Stateflows Minimal APIs: endpoint {key} for {description} conflicts with endpoint for {existing}. Change the route of one of them using endpoint configuration or an interceptor.");
            }

            routes.Add(key, description);
        }

        public override bool BeforeEventEndpointDefinition<TEvent>(BehaviorClass behaviorClass, bool isDefaultInstance, ref string method, ref string route)
        {
            Register(method, route, $"{behaviorClass.Type} '{behaviorClass.Name}' event '{Event<TEvent>.Name}'");
            return true;
        }

        public override bool BeforeGetInstancesEndpointDefinition(BehaviorClass behaviorClass, ref string method, ref string route)
        {
            Register(method, route, $"{behaviorClass.Type} '{behaviorClass.Name}' instances");
            return true;
        }
    }
}
