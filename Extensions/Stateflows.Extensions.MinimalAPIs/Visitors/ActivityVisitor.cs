using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;

namespace Stateflows.Extensions.MinimalAPIs;

internal class ActivityVisitor(
    IEndpointRouteBuilder routeBuilder,
    Interceptor interceptor,
    ITypeMapper typeMapper,
    IPayloadsRegister eventsRegister
) : BehaviorVisitor(routeBuilder, typeMapper), IActivityVisitor, IBehaviorClassVisitor, ITypeVisitor
{
    public IEndpointRouteBuilder RouteBuilder => routeBuilder;
    public Interceptor Interceptor => interceptor;
    private readonly Dictionary<string, bool> Initializers = new();

    private string CurrentActivityName = string.Empty;
    private BehaviorClass? OwnerClass = null;

    private readonly Dictionary<(string Name, int Version), Dictionary<(bool Input, Type Type, string? PayloadName), System.Action>> pinEndpoints = new();
    private readonly Dictionary<string, int> latestPinVersions = new();
    private readonly HashSet<(string Name, int Version, string ElementId, TokenNamesDeclarationKind Kind)> rootPayloadPins = [];

    public Task InputPinAddedAsync<TToken>(string pinId, string activityName, int activityVersion, string? parentNodeId)
    {
        if (OwnerClass == null && parentNodeId == null && typeof(TToken) == typeof(Payload))
        {
            rootPayloadPins.Add((activityName, activityVersion, pinId, TokenNamesDeclarationKind.InputPin));
        }
        else if (OwnerClass == null && parentNodeId == null && ActivityPinEndpoints.IsMappable<TToken>())
        {
            var hasDefaultInstance = HasDefaultInstance;
            pinEndpoints[(activityName, activityVersion)][(true, typeof(TToken), null)] = () =>
                routeBuilder.RegisterInputPinEndpoint<TToken>(interceptor, activityName, hasDefaultInstance, HateoasLinks);
        }
        return Task.CompletedTask;
    }

    public Task OutputPinAddedAsync<TToken>(string pinId, string activityName, int activityVersion, string? parentNodeId)
    {
        if (OwnerClass == null && parentNodeId == null && typeof(TToken) == typeof(Payload))
        {
            rootPayloadPins.Add((activityName, activityVersion, pinId, TokenNamesDeclarationKind.OutputPin));
        }
        else if (OwnerClass == null && parentNodeId == null && ActivityPinEndpoints.IsMappable<TToken>())
        {
            var hasDefaultInstance = HasDefaultInstance;
            pinEndpoints[(activityName, activityVersion)][(false, typeof(TToken), null)] = () =>
                routeBuilder.RegisterOutputPinEndpoint<TToken>(interceptor, activityName, hasDefaultInstance, HateoasLinks);
        }
        return Task.CompletedTask;
    }

    public Task TokenNamesDeclaredAsync(string elementId, string activityName, int activityVersion,
        TokenNamesDeclarationKind kind, IReadOnlyList<string> tokenNames)
    {
        if (!rootPayloadPins.Contains((activityName, activityVersion, elementId, kind)))
            return Task.CompletedTask;

        foreach (var tokenName in tokenNames)
        {
            var definition = eventsRegister.GetPayload(tokenName);
            if (definition.IsRequest)
                continue;

            var hasDefaultInstance = HasDefaultInstance;
            var isInput = kind == TokenNamesDeclarationKind.InputPin;
            pinEndpoints[(activityName, activityVersion)][(isInput, typeof(Payload), tokenName)] = () =>
                routeBuilder.RegisterPayloadPinEndpoint(interceptor, definition, activityName, isInput, hasDefaultInstance, HateoasLinks);
        }

        return Task.CompletedTask;
    }

    internal void RegisterPinEndpoints()
    {
        foreach (var (name, version) in latestPinVersions)
            foreach (var register in pinEndpoints[(name, version)].Values)
                register();
    }

    public bool HasDefaultInstance { get; private set; } = false;

    public void Visit<T>()
    {
        RegisterEventEndpoint<T>(CurrentActivityName);
    }

    public static Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> HateoasLinks { get; } = new();

    Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> IBehaviorClassVisitor.HateoasLinks => HateoasLinks;

    Task IActivityVisitor.ActivityTypeAddedAsync<TActivity>(string activityId, string activityName, int activityVersion)
    {
        return ActivityTypeAddedAsync<TActivity>(activityId, activityName, activityVersion);
    }

    public Task InitializerAddedAsync<TInitializationEvent>(string initializerId, string activityName, int activityVersion, IReadOnlyList<string> eventNames, int order)
    {
        if (OwnerClass != null)
        {
            return Task.CompletedTask;
        }

        Initializers[activityName] = true;

        if (typeof(TInitializationEvent) == typeof(Payload))
        {
            foreach (var eventName in eventNames)
                if (PayloadTriggers.Add((activityName, eventName)))
                    RouteBuilder.RegisterPayloadEndpoint(Interceptor, BehaviorType.Activity, activityName, eventsRegister.GetPayload(eventName), HateoasLinks, HasDefaultInstance);
            return Task.CompletedTask;
        }

        CurrentActivityName = activityName;
        typeMapper.VisitMappedTypes<TInitializationEvent>(this);
        CurrentActivityName = string.Empty;

        return Task.CompletedTask;
    }



    Task IActivityVisitor.NodeAddedAsync(string nodeId, string activityName, int activityVersion, string nodeName, NodeType nodeType,
        string? parentNodeId)
    {
        nodeNames[(activityName, activityVersion, nodeId)] = nodeName;
        NodeTypes[(activityName, nodeName)] = nodeType;
        return Task.CompletedTask;
    }

    private readonly Dictionary<(string Name, int Version, string Id), string> nodeNames = new();

    // Node types are taken from the model, so endpoint contexts don't need to hydrate behavior state.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<(string ActivityName, string NodeName), NodeType> NodeTypes = new();

    internal static NodeType GetNodeType(string activityName, string nodeName)
        => NodeTypes.TryGetValue((activityName, nodeName), out var nodeType) ? nodeType : default;

    Task IActivityVisitor.NodeTypeAddedAsync<TNode>(string nodeId, string activityName, int activityVersion)
    {
        return RegisterNodeEndpoints<TNode>(activityName, activityVersion, nodeNames[(activityName, activityVersion, nodeId)]);
    }

    public Task AcceptEventNodeAddedAsync<TEvent>(string nodeId, string activityName, int activityVersion, IReadOnlyList<string> eventNames)
    {
        if (OwnerClass != null)
        {
            return Task.CompletedTask;
        }

        if (typeof(TEvent) == typeof(Payload))
        {
            foreach (var eventName in eventNames)
                if (PayloadTriggers.Add((activityName, eventName)))
                    RouteBuilder.RegisterPayloadEndpoint(Interceptor, BehaviorType.Activity, activityName, eventsRegister.GetPayload(eventName), HateoasLinks, HasDefaultInstance);
            return Task.CompletedTask;
        }

        CurrentActivityName = activityName;
        typeMapper.VisitMappedTypes<TEvent>(this);
        CurrentActivityName = string.Empty;

        return Task.CompletedTask;
    }

    public Task ActivityAddingAsync(string activityId, string activityName, int activityVersion, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false)
    {
        HasDefaultInstance = hasDefaultInstance;
        OwnerClass = ownerClass;
        if (ownerClass == null)
        {
            pinEndpoints[(activityName, activityVersion)] = new();
            latestPinVersions[activityName] = Math.Max(latestPinVersions.GetValueOrDefault(activityName), activityVersion);
        }

        return Task.CompletedTask;
    }

    public Task ActivityAddedAsync(string activityId, string activityName, int activityVersion)
    {
        if (OwnerClass != null)
        {
            return Task.CompletedTask;
        }

        RegisterStandardEndpoints(activityName);
        RegisterRemainingEndpoints(activityName);

        if (HasDefaultInstance)
        {
            RegisterDefaultInstanceEndpoints(activityName);
        }

        return Task.CompletedTask;
    }

    public Task ActivityTypeAddedAsync<TActivity>(string activityId, string activityName, int activityVersion)
        where TActivity : class, IActivity
    {
        if (OwnerClass != null)
        {
            return Task.CompletedTask;
        }

        var activityType = typeof(TActivity);
        if (typeof(IActivityEndpoints).IsAssignableFrom(activityType))
        {
            var endpointsBuilder = new EndpointsBuilder(routeBuilder, this, interceptor, new ActivityClass(activityName), HasDefaultInstance);

            activityType.CallStaticMethod(nameof(IActivityEndpoints.RegisterEndpoints), [typeof(IEndpointsBuilder)], [endpointsBuilder]);

            // RegisterEndpoints<TActivity>(endpointsBuilder);

            // var activity = (IActivityEndpoints)StateflowsActivator.CreateUninitializedInstance<TActivity>();
            // activity.RegisterEndpoints(endpointsBuilder);
        }

        return Task.CompletedTask;
    }

    protected override void RegisterEventEndpoint<TEvent>(string activityName, IEndpointRouteBuilder activity, BehaviorStatus[]? supportedStatuses = null)
        => activity.RegisterEventEndpoint<TEvent>(interceptor, BehaviorType.Activity, activityName, HateoasLinks, HasDefaultInstance, supportedStatuses);

    protected override void RegisterRequestEndpoint<TRequest, TResponse>(string activityName, IEndpointRouteBuilder activity, BehaviorStatus[]? supportedStatuses = null)
        => activity.RegisterRequestEndpoint<TRequest, TResponse>(interceptor, BehaviorType.Activity, activityName, HateoasLinks, HasDefaultInstance, supportedStatuses);

    private void RegisterRemainingEndpoints(string activityName)
    {
        if (Initializers.TryGetValue(activityName, out var hasInitializers) && hasInitializers)
        {
            return;
        }

        RegisterEventEndpoint<Initialize>(activityName, [BehaviorStatus.NotInitialized]);
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
            var routeHandlerBuilder = routeBuilder.MapMethods(route, [method], async (IStateflowsStorage storage, ITenantAccessor tenantAccessor) =>
            {
                BehaviorClass[] actionClasses = [behaviorClass];
                tenantAccessor.CurrentTenantId ??= "host";
                var contextIds = await storage.GetAllContextIdsAsync(actionClasses);
                return Results.Ok(contextIds.Select(id => new { Id = id }));
            })
            .WithTags($"{BehaviorType.Activity} {activityName}");

            interceptor.AfterGetInstancesEndpointDefinition(behaviorClass, method, route, routeHandlerBuilder);
        }

        route = $"/activities/{activityName}/{{instance}}/status";
        method = HttpMethods.Get;
        if (interceptor.BeforeEventEndpointDefinition<ActivityInfoRequest>(behaviorClass, isDefaultInstance: false, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(
                route,
                [method],
                async (
                    string instance,
                    IActivityLocator locator,
                    HttpContext httpContext,
                    [FromQuery] bool implicitInitialization = false,
                    [FromQuery] bool stream = false
                ) =>
                {
                    if (locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior))
                    {
                        if (stream)
                        {
                            httpContext.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");

                            await using var watcher = await behavior.WatchAsync(
                                [Event<ActivityInfo>.Name],
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
                            var requestResult =
                                await behavior.GetStatusAsync(implicitInitialization
                                    ? []
                                    : new Dictionary<string, EventHeader>() { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } });
                            // workaround for return code 200 regardless behavior actual status
                            requestResult.Status = EventStatus.Consumed;
                            return requestResult.ToResult(method ?? "[no method]", route ?? "[no path]", [], requestResult.Response, HateoasLinks);
                        }
                    }

                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.Activity} {activityName}");

            interceptor.AfterEventEndpointDefinition<ActivityInfoRequest>(behaviorClass, isDefaultInstance: false, method, route, routeHandlerBuilder);

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
        if (interceptor.BeforeEventEndpointDefinition<NotificationsRequest>(behaviorClass, isDefaultInstance: false, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(
                route,
                [method],
                async (
                    string instance,
                    IActivityLocator locator,
                    HttpContext httpContext,
                    [FromQuery] string[] names,
                    [FromQuery] TimeSpan? period,
                    [FromQuery] bool stream = false
                ) =>
                {
                    if (locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior))
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
                            return sendResult.ToResult(method ?? "[no method]", route ?? "[no path]", notifications, behaviorInfo, HateoasLinks);
                        }
                    }

                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.Activity} {activityName}");

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

        route = $"/activities/{activityName}/{{instance}}/finalize";
        method = HttpMethods.Post;
        if (interceptor.BeforeEventEndpointDefinition<Finalize>(behaviorClass, isDefaultInstance: false, ref method, ref route))
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
                        // var compoundResult = await behavior.SendCompoundAsync(b => b
                        //     .Add(new Finalize())
                        //     .Add(new ActivityInfoRequest(), new Dictionary<string, EventHeader>() { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } })
                        // );
                        //
                        // var result = compoundResult.Response.Results.First();
                        // var behaviorInfo = ((EventHolder<ActivityInfo>)compoundResult.Response.Results.Last().Response).Payload;
                        //
                        // return result.ToResult(method ?? "[no method]", route ?? "[no path]", [], behaviorInfo, HateoasLinks);

                        var sendResult = await behavior.FinalizeAsync();
                        var behaviorInfo = (await behavior.GetStatusAsync(new Dictionary<string, EventHeader>() { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } })).Response;
                        return sendResult.ToResult(method ?? "[no method]", route ?? "[no path]", [], behaviorInfo, HateoasLinks);
                    }

                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.Activity} {activityName}");

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

        route = $"/activities/{activityName}/{{instance}}";
        method = HttpMethods.Delete;
        if (interceptor.BeforeEventEndpointDefinition<Reset>(behaviorClass, isDefaultInstance: false, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(
                route,
                [method],
                async (string instance, IActivityLocator locator) =>
                {
                    if (locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior))
                    {
                        // var compoundResult = await behavior.SendCompoundAsync(b => b
                        //     .Add(new Reset())
                        //     .Add(new BehaviorInfoRequest(), new Dictionary<string, EventHeader>() { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } })
                        // );
                        //
                        // var result = compoundResult.Response.Results.First();
                        // var behaviorInfo = ((EventHolder<BehaviorInfo>)compoundResult.Response.Results.Last().Response).Payload;
                        //
                        // return result.ToResult(method ?? "[no method]", route ?? "[no path]", [], behaviorInfo, HateoasLinks);

                        var sendResult = await behavior.ResetAsync();
                        var behaviorInfo = (await behavior.GetStatusAsync(new Dictionary<string, EventHeader>() { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } })).Response;
                        return sendResult.ToResult(method ?? "[no method]", route ?? "[no path]", [], behaviorInfo, HateoasLinks);
                    }

                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.Activity} {activityName}");

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
    }

    private void RegisterDefaultInstanceEndpoints(string activityName)
    {
        var behaviorClass = new ActivityClass(activityName);
        const string instance = "";

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

        route = $"/activities/{activityName}/status";
        method = HttpMethods.Get;
        if (interceptor.BeforeEventEndpointDefinition<ActivityInfoRequest>(behaviorClass, isDefaultInstance: true, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(
                route,
                [method],
                async (
                    IActivityLocator locator,
                    HttpContext httpContext,
                    [FromQuery] bool implicitInitialization = false,
                    [FromQuery] bool stream = false
                ) =>
                {
                    if (locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior))
                    {
                        if (stream)
                        {
                            httpContext.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");

                            await using var watcher = await behavior.WatchAsync(
                                [Event<ActivityInfo>.Name],
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
                            var requestResult =
                                await behavior.GetStatusAsync(implicitInitialization
                                    ? []
                                    : new Dictionary<string, EventHeader>() { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } });
                            // workaround for return code 200 regardless behavior actual status
                            requestResult.Status = EventStatus.Consumed;
                            return requestResult.ToResult(method ?? "[no method]", route ?? "[no path]", [], requestResult.Response, HateoasLinks);
                        }
                    }

                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.Activity} {activityName}");

            interceptor.AfterEventEndpointDefinition<ActivityInfoRequest>(behaviorClass, isDefaultInstance: true, method, route, routeHandlerBuilder);

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

        route = $"/activities/{activityName}/notifications";
        method = HttpMethods.Get;
        if (interceptor.BeforeEventEndpointDefinition<NotificationsRequest>(behaviorClass, isDefaultInstance: true, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(
                route,
                [method],
                async (
                    IActivityLocator locator,
                    HttpContext httpContext,
                    [FromQuery] string[] names,
                    [FromQuery] TimeSpan? period,
                    [FromQuery] bool stream = false
                ) =>
                {
                    if (locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior))
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
                            return sendResult.ToResult(method ?? "[no method]", route ?? "[no path]", notifications, behaviorInfo, HateoasLinks);
                        }
                    }

                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.Activity} {activityName}");

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

    private Task RegisterNodeEndpoints<TNode>(string activityName, int activityVersion, string nodeName)
        where TNode : class, IActivityNode
    {
        if (OwnerClass != null)
        {
            return Task.CompletedTask;
        }

        var nodeType = typeof(TNode);
        if (typeof(IStructuredActivityNodeEndpoints).IsAssignableFrom(nodeType))
        {
            var behaviorClass = new ActivityClass(activityName);

            DependencyInjection.ActivityEndpointBuilders.Add(visitor =>
            {
                var endpointsBuilder = new EndpointsBuilder(routeBuilder, visitor, interceptor, behaviorClass, HasDefaultInstance, nodeName);

                nodeType.CallStaticMethod(nameof(IStructuredActivityNodeEndpoints.RegisterEndpoints), [typeof(IEndpointsBuilder)], [endpointsBuilder]);
                // RegisterEndpoints<TNode>(endpointsBuilder);

                // var node = (IStructuredActivityNodeEndpoints)StateflowsActivator.CreateUninitializedInstance<TNode>();
                // node.RegisterEndpoints(endpointsBuilder);
            });
        }

        return Task.CompletedTask;
    }


}