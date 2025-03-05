using System.Text.Json;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines;

namespace Stateflows.Transport.REST;

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
        if (typeof(SystemEvent).IsAssignableFrom(eventType))
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

    private static string GetEventName<TEvent>()
        => JsonNamingPolicy.CamelCase.ConvertName(Event<TEvent>.Name.Split('.').Last());

    private void RegisterEventEndpoint<TEvent>(string stateMachineName, RouteGroupBuilder stateMachine)
    {
        routeHandlerBuilderAction(
            stateMachine.MapPost("/{instance}/" + GetEventName<TEvent>(),
                async (string instance, IStateMachineLocator locator, TEvent @event)
                    => locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance), out var behavior)
                        ? (await behavior.SendAsync(@event)).ToResult()
                        : Results.NotFound()
            )
        );
    }

    private void RegisterRequestEndpoint<TRequest, TResponse>(string stateMachineName, RouteGroupBuilder stateMachine)
        where TRequest : IRequest<TResponse>
    {
        routeHandlerBuilderAction(
            stateMachine.MapPost("/{instance}/" + GetEventName<TRequest>(),
                async (string instance, IStateMachineLocator locator, TRequest request)
                    => locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance), out var behavior)
                        ? (await behavior.RequestAsync(request)).ToResult()
                        : Results.NotFound()
            )
        );
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
            stateMachine.MapGet("/{instance}/status", async (string instance, IStateMachineLocator locator)
                => locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance), out var behavior)
                    ? (await behavior.GetStatusAsync()).ToResult()
                    : Results.NotFound()
            )
        );

        routeHandlerBuilderAction(
            stateMachine.MapGet("/{instance}/currentState", async (string instance, IStateMachineLocator locator)
                => locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance), out var behavior)
                    ? (await behavior.GetCurrentStateAsync()).ToResult()
                    : Results.NotFound()
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
                    return locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance),
                        out var behavior)
                        ? (await behavior.RequestAsync(new NotificationsRequest()
                        {
                            Period = (TimeSpan)period,
                            NotificationNames = names.ToList()
                        })).ToResult()
                        : Results.NotFound();
                })
        );

        routeHandlerBuilderAction(
            stateMachine.MapPost("/{instance}/finalize", async (string instance, IStateMachineLocator locator)
                => locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance), out var behavior)
                    ? (await behavior.FinalizeAsync()).ToResult()
                    : Results.NotFound()
            )
        );

        routeHandlerBuilderAction(
            stateMachine.MapDelete("/{instance}", async (string instance, IStateMachineLocator locator)
                => locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance), out var behavior)
                    ? (await behavior.ResetAsync()).ToResult()
                    : Results.NotFound()
            )
        );
    }
}