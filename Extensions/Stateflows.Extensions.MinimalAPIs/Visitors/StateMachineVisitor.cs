using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Extensions.MinimalAPIs.Attributes;
using Stateflows.StateMachines;

namespace Stateflows.Extensions.MinimalAPIs;

internal class StateMachineVisitor(
    RouteGroupBuilder behaviorsBuilder,
    Action<RouteHandlerBuilder> routeHandlerBuilderAction,
    ITypeMapper typeMapper
) : StateMachines.StateMachineVisitor, IBehaviorClassVisitor, ITypeVisitor
{
    private readonly Dictionary<string, RouteGroupBuilder> RouteGroups = new();
    private readonly Dictionary<string, bool> Initializers = new();
    private readonly Dictionary<string, List<Type>> Triggers = new();
    private static readonly MethodInfo RegisterRequestMethod = typeof(StateMachineVisitor).GetMethod(
        nameof(RegisterRequestEndpoint),
        BindingFlags.Instance | BindingFlags.NonPublic
    )!;

    private string CurrentStateMachineName = string.Empty;
    
    public void Visit<T>()
    {
        RegisterEventEndpoint<T>(CurrentStateMachineName);
    }

    public RouteGroupBuilder GetRouteGroup(string behaviorName)
    {
        if (!RouteGroups.TryGetValue(behaviorName, out var stateMachine))
        {
            stateMachine = behaviorsBuilder.MapGroup($"/{behaviorName}");
            RouteGroups.Add(behaviorName, stateMachine);
            
            RegisterStandardEndpoints(behaviorName, stateMachine);
        }

        return stateMachine;
    }

    public Dictionary<string, List<HateoasLink>> CustomHateoasLinks { get; set; } = new();
    public void AddLink(HateoasLink link, string scope = "")
    {
        if (!CustomHateoasLinks.TryGetValue(scope, out var links))
        {
            links = new List<HateoasLink>();
            CustomHateoasLinks.Add(scope, links);
        }
        
        links.Add(link);
    }

    public override Task InitializerAddedAsync<TInitializationEvent>(string stateMachineName, int stateMachineVersion)
    {
        Initializers[stateMachineName] = true;
        
        CurrentStateMachineName = stateMachineName;
        typeMapper.VisitMappedTypes<TInitializationEvent>(this);
        CurrentStateMachineName = string.Empty;
        
        // RegisterEventEndpoint<TInitializationEvent>(stateMachineName);

        return Task.CompletedTask;
    }

    public override Task TransitionAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, string sourceVertexName,
        string targetVertexName = null, bool isElse = false)
    {
        CurrentStateMachineName = stateMachineName;
        typeMapper.VisitMappedTypes<TEvent>(this);
        CurrentStateMachineName = string.Empty;
        
        // RegisterEventEndpoint<TEvent>(stateMachineName);

        return Task.CompletedTask;
    }

    public override Task StateMachineAddedAsync(string stateMachineName, int stateMachineVersion)
    {
        RegisterRemainingEndpoints();
        
        return Task.CompletedTask;
    }

    private void RegisterEventEndpoint<TEvent>(string stateMachineName)
    {
        var stateMachine = GetRouteGroup(stateMachineName);

        var eventType = typeof(TEvent);
        if (
            eventType.GetCustomAttributes<NoApiMappingAttribute>().Any() ||
            eventType.IsAssignableTo(typeof(SystemEvent)) ||
            eventType.IsAssignableTo(typeof(Exception))
        )
        {
            return;
        }

        if (!Triggers.TryGetValue(stateMachineName, out var triggers))
        {
            triggers = new List<Type>();
            Triggers.Add(stateMachineName, triggers);
        }

        if (!triggers.Contains(typeof(TEvent)))
        {
            triggers.Add(eventType);
            
            if (eventType.IsRequest())
            {
                var responseType = eventType
                    .GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
                    .GetGenericArguments()
                    .First();
                
                var method = RegisterRequestMethod.MakeGenericMethod(eventType, responseType);
                method.Invoke(
                    this,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    [stateMachineName, stateMachine], 
                    null
                );
            }
            else
            {
                RegisterEventEndpoint<TEvent>(stateMachineName, stateMachine);
            }
        }
    }

    private void RegisterRemainingEndpoints()
    {
        foreach (var stateMachineName in RouteGroups.Keys)
        {
            if (Initializers.TryGetValue(stateMachineName, out var hasInitializers) && hasInitializers)
            {
                continue;
            }
            
            RegisterEventEndpoint<Initialize>(stateMachineName);
        }
    }

    private void RegisterEventEndpoint<TEvent>(string stateMachineName, RouteGroupBuilder stateMachine)
    {
        var eventType = typeof(TEvent);
        if (Utils.IsEventEmpty(eventType))
        {
            routeHandlerBuilderAction(
                stateMachine.MapPost("/{instance}/" + Utils.GetEventName<TEvent>(),
                    async (
                        HttpContext context,
                        IServiceProvider serviceProvider,
                        string instance,
                        IStateMachineLocator locator,
                        RequestBody payload,
                        [FromQuery] bool implicitInitialization = true
                    ) =>
                    {
                        var (success, authorizationResult) = await Utils.AuthorizeEventAsync(eventType, serviceProvider, context);
                        if (!success)
                        {
                            return authorizationResult;
                        }

                        if (locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance), out var behavior))
                        {
                            var result = await behavior.SendAsync(StateflowsActivator.CreateUninitializedInstance(eventType), implicitInitialization ? [] : [new NoImplicitInitialization()]);
                        
                            var notifications = (await behavior.GetNotificationsAsync(payload.RequestedNotifications)).Response.Notifications.ToArray();
                            var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;

                            return result.ToResult(notifications, behaviorInfo, CustomHateoasLinks);
                        }
                    
                        return Results.NotFound();
                    }
                )
            );
        }
        else
        {
            routeHandlerBuilderAction(
                stateMachine.MapPost("/{instance}/" + Utils.GetEventName<TEvent>(),
                    async (
                        HttpContext context,
                        IServiceProvider serviceProvider,
                        string instance,
                        IStateMachineLocator locator,
                        RequestBody<TEvent> payload,
                        [FromQuery] bool implicitInitialization = true
                    ) =>
                    {
                        var (success, authorizationResult) = await Utils.AuthorizeEventAsync(eventType, serviceProvider, context);
                        if (!success)
                        {
                            return authorizationResult;
                        }

                        if (locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance), out var behavior))
                        {
                            var result = payload.Event == null
                                ? new SendResult(
                                    EventStatus.Invalid,
                                    new EventValidation(false, [ new ValidationResult("Event not provided") ])
                                )
                                : await behavior.SendAsync(payload.Event, implicitInitialization ? [] : [new NoImplicitInitialization()]);
                        
                            var notifications = (await behavior.GetNotificationsAsync(payload.RequestedNotifications)).Response.Notifications.ToArray();
                            var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;

                            return result.ToResult(notifications, behaviorInfo, CustomHateoasLinks);
                        }
                    
                        return Results.NotFound();
                    }
                )
            );
        }
    }

    private void RegisterRequestEndpoint<TRequest, TResponse>(string stateMachineName, RouteGroupBuilder stateMachine)
        where TRequest : IRequest<TResponse>
    {
        var eventType = typeof(TRequest);
        if (Utils.IsEventEmpty(eventType))
        {
            routeHandlerBuilderAction(
                stateMachine.MapPost("/{instance}/" + Utils.GetEventName<TRequest>(),
                    async (
                        HttpContext context,
                        IServiceProvider serviceProvider,
                        string instance,
                        IStateMachineLocator locator,
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

                        if (locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance),
                                out var behavior))
                        {
                            var result = await behavior.RequestAsync((TRequest)StateflowsActivator.CreateUninitializedInstance(eventType),
                                    implicitInitialization ? [] : [new NoImplicitInitialization()]);

                            var notifications = (await behavior.GetNotificationsAsync(payload.RequestedNotifications))
                                .Response.Notifications.ToArray();
                            var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()]))
                                .Response;
                            return result.ToResult(notifications, behaviorInfo, CustomHateoasLinks);
                        }

                        return Results.NotFound();
                    }
                )
            );
        }
        else
        {
            routeHandlerBuilderAction(
                stateMachine.MapPost("/{instance}/" + Utils.GetEventName<TRequest>(),
                    async (
                        HttpContext context,
                        IServiceProvider serviceProvider,
                        string instance,
                        IStateMachineLocator locator,
                        RequestBody<TRequest> payload,
                        [FromQuery] bool implicitInitialization = true
                    ) =>
                    {
                        var (success, authorizationResult) = await Utils.AuthorizeEventAsync(eventType, serviceProvider, context);
                        if (!success)
                        {
                            return authorizationResult;
                        }
                        
                        if (locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance), out var behavior))
                        {
                            var result = payload.Event == null
                                ? new SendResult(
                                    EventStatus.Invalid,
                                    new EventValidation(false, [ new ValidationResult("Event not provided") ])
                                )
                                : await behavior.RequestAsync(payload.Event, implicitInitialization ? [] : [new NoImplicitInitialization()]);
                            
                            var notifications = (await behavior.GetNotificationsAsync(payload.RequestedNotifications)).Response.Notifications.ToArray();
                            var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                            return result.ToResult(notifications, behaviorInfo, CustomHateoasLinks);
                        }
                        
                        return Results.NotFound();
                    }
                )
            );
        }
    }

    private void RegisterStandardEndpoints(string stateMachineName, RouteGroupBuilder stateMachine)
    {
        routeHandlerBuilderAction(
            stateMachine.MapGet("/", async (IStateflowsStorage storage) =>
            {
                IEnumerable<BehaviorClass> actionClasses = [new StateMachineClass(stateMachineName)];
                var contexts = await storage.GetAllContextsAsync(actionClasses);
                return Results.Ok(contexts.Select(context => context.Id));
            })
        );

        routeHandlerBuilderAction(
            stateMachine.MapGet("/{instance}/status",
                async (
                    string instance,
                    IStateMachineLocator locator
                ) =>
                {
                    if (locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance), out var behavior))
                    {
                        var result = await behavior.GetStatusAsync([new NoImplicitInitialization()]);
                        // workaround for return code 200 regardless behavior actual status
                        result.Status = EventStatus.Consumed;
                        return result.ToResult([], result.Response, CustomHateoasLinks);
                    }

                    return Results.NotFound();
                }
            )
        );

        routeHandlerBuilderAction(
            stateMachine.MapGet(
                "/{instance}/notifications",
                async (
                    IStateMachineLocator locator,
                    string instance,
                    [FromQuery] string[] names,
                    [FromQuery] TimeSpan? period
                ) =>
                {
                    period ??= TimeSpan.FromSeconds(60);
                    if (locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance), out var behavior))
                    {
                        var result = await behavior.GetNotificationsAsync(names, period, [new NoImplicitInitialization()]);
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return result.ToResult(result.Response.Notifications, behaviorInfo, CustomHateoasLinks);
                    }
                    return Results.NotFound();
                })
        );

        routeHandlerBuilderAction(
            stateMachine.MapPost("/{instance}/finalize",
                async (
                    string instance,
                    IStateMachineLocator locator
                ) =>
                {
                    if (locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance), out var behavior))
                    {
                        var result = await behavior.FinalizeAsync();
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return result.ToResult([], behaviorInfo, CustomHateoasLinks);
                    }
                    
                    return Results.NotFound();
                }
            )
        );

        routeHandlerBuilderAction(
            stateMachine.MapDelete("/{instance}", 
                async (
                    string instance,
                    IStateMachineLocator locator
                ) =>
                {
                    if (locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance), out var behavior))
                    {
                        var result = await behavior.ResetAsync();
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return result.ToResult([], behaviorInfo, CustomHateoasLinks);
                    }
                    
                    return Results.NotFound();
                }
            )
        );
    }

    public override Task StateMachineTypeAddedAsync<TStateMachine>(string stateMachineName, int stateMachineVersion)
    {
        if (typeof(IStateMachineEndpoints).IsAssignableFrom(typeof(TStateMachine)))
        {
            var endpointsBuilder = new EndpointsBuilder(this, new StateMachineClass(stateMachineName));
            
            var stateMachine = (IStateMachineEndpoints)StateflowsActivator.CreateUninitializedInstance<TStateMachine>();
            stateMachine.RegisterEndpoints(endpointsBuilder);
        }
        
        return Task.CompletedTask;
    }

    public override Task VertexTypeAddedAsync<TVertex>(string stateMachineName, int stateMachineVersion, string vertexName)
    {
        if (typeof(IStateEndpoints).IsAssignableFrom(typeof(TVertex)))
        {
            var behaviorClass = new StateMachineClass(stateMachineName);
            
            DependencyInjection.StateMachineEndpointBuilders.Add(visitor =>
            {
                var builder = new EndpointsBuilder(visitor, behaviorClass, vertexName);
                
                var state = (IStateEndpoints)StateflowsActivator.CreateUninitializedInstance<TVertex>();
                state.RegisterEndpoints(builder);
            });
        }

        return Task.CompletedTask;
    }
}