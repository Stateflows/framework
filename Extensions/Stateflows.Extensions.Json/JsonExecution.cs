using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.StateMachines.Context;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Json;

internal static class JsonExecution
{
    private static void BuildMachine<T>(StateMachineBuilder builder) where T : class, IStateMachine
        => T.Build(builder);

    private static void BuildState<T>(Vertex vertex) where T : class, IStateDefinition
        => T.Build(new StateBuilder(vertex));

    private static void BuildComposite<T>(Vertex vertex) where T : class, ICompositeStateDefinition
        => T.Build(new CompositeStateBuilder(vertex.DefaultRegion));

    private static void BuildOrthogonal<T>(Vertex vertex) where T : class, IOrthogonalStateDefinition
        => T.Build(new OrthogonalStateBuilder(vertex));

    private static void BuildTransition<TEvent>(ClrBinding binding, Edge edge)
        => ((ITransitionDefinition<TEvent>)binding.CreateDefinition()).Build(new TransitionBuilder<TEvent>(edge));

    private static Func<IStateMachineVisitor, Task> TransitionVisit<TEvent>(Edge edge)
        => visitor => visitor.TransitionAddedAsync<TEvent>(edge.Graph.Name, edge.Graph.Version, edge.SourceName,
            edge.TargetName == "" ? null : edge.TargetName, edge.IsElse);

    private static void AddStateHandler<T>(ClrBinding binding, Vertex vertex, string role) where T : class, IState
    {
        var builder = new StateBuilder(vertex);
        async Task Execute(IStateActionContext context)
        {
            var instance = await binding.ConfigureAsync(((BaseContext)context).Context.Executor.GetStateAsync<T>(context));
            await (role switch
            {
                "onEntry" => ((IStateEntry)instance).OnEntryAsync(),
                "onExit" => ((IStateExit)instance).OnExitAsync(),
                "onInitialize" => ((ICompositeStateInitialization)instance).OnInitializeAsync(),
                _ => ((ICompositeStateFinalization)instance).OnFinalizeAsync()
            });
        }
        switch (role)
        {
            case "onEntry": builder.AddOnEntry(Execute); break;
            case "onExit": builder.AddOnExit(Execute); break;
            case "onInitialize": builder.AddOnInitialize(Execute); break;
            default: builder.AddOnFinalize(Execute); break;
        }
    }

    internal static void AddStateBehavior(Vertex vertex, string role, BehaviorClass behavior)
    {
        var builder = new StateBuilder(vertex);
        Task Execute(IStateActionContext context) => behavior.Type == BehaviorType.Activity
            ? StateMachineActivityExtensions.RunStateActivityAsync(role, context, behavior.Name)
            : StateMachineActionExtensions.RunStateActionAsync(role, context, behavior.Name);
        switch (role)
        {
            case "onEntry": builder.AddOnEntry(Execute); break;
            case "onExit": builder.AddOnExit(Execute); break;
            case "onInitialize": builder.AddOnInitialize(Execute); break;
            default: builder.AddOnFinalize(Execute); break;
        }
    }

    private static Func<RootContext, Edge, Task<bool>> TransitionGuard<T, TEvent>(ClrBinding binding)
        where T : class, ITransitionGuard<TEvent>
        => async (root, edge) =>
        {
            var context = new GuardContext<TEvent>(root, edge);
            var instance = await binding.ConfigureAsync(root.Executor.GetTransitionGuardAsync<T, TEvent>(context));
            return await instance.GuardAsync(context.Event);
        };

    private static Func<RootContext, Edge, Task> TransitionEffect<T, TEvent>(ClrBinding binding)
        where T : class, ITransitionEffect<TEvent>
        => async (root, edge) =>
        {
            var context = new TransitionContext<TEvent>(root, edge);
            var instance = await binding.ConfigureAsync(root.Executor.GetTransitionEffectAsync<T, TEvent>(context));
            await instance.EffectAsync(context.Event);
        };

    private static Func<RootContext, Vertex, Task<bool>> DeferralGuard<T, TEvent>(ClrBinding binding)
        where T : class, IDeferralGuard<TEvent>
        => async (root, vertex) =>
        {
            var context = new DeferralContext<TEvent>(root, vertex);
            var instance = await binding.ConfigureAsync(root.Executor.GetDeferralGuardAsync<T, TEvent>(context));
            return await instance.GuardAsync(context.Event);
        };

    internal static Task RunEffect(ITransitionContext<object> context, BehaviorClass behavior)
        => behavior.Type == BehaviorType.Activity
            ? StateMachineActivityExtensions.RunEffectActivity(context, behavior.Name)
            : StateMachineActionExtensions.RunEffectActionAsync(context, behavior.Name);

    internal static Task<bool> RunTransitionGuard(RootContext root, Edge edge, BehaviorClass behavior, int index)
    {
        var context = new TransitionContext<object>(root, edge);
        return behavior.Type == BehaviorType.Activity
            ? StateMachineActivityExtensions.RunTransitionGuardActivityAsync(index, context, behavior.Name)
            : StateMachineActionExtensions.RunTransitionGuardActionAsync(index, context, behavior.Name);
    }

    private static Func<RootContext, Vertex, Task<bool>> DeferralBehavior<TEvent>(BehaviorClass behavior, int index)
        => (root, vertex) =>
        {
            var context = new DeferralContext<TEvent>(root, vertex);
            return behavior.Type == BehaviorType.Activity
                ? StateMachineActivityExtensions.RunDeferralGuardActivityAsync(index, context, behavior.Name)
                : StateMachineActionExtensions.RunDeferralGuardActionAsync(index, context, behavior.Name);
        };

    private static void AddDefaultInitializer<T>(ClrBinding binding, StateMachineBuilder builder)
        where T : class, IDefaultInitializer
        => builder.AddDefaultInitializer(async context =>
        {
            var instance = await binding.ConfigureAsync(((BaseContext)context).Context.Executor.GetDefaultInitializerAsync<T>(context));
            return await instance.OnInitializeAsync();
        });

    private static void AddInitializer<T, TEvent, TTrigger>(ClrBinding binding, StateMachineBuilder builder)
        where T : class, IInitializer<TEvent>
        => builder.AddInitializer<TTrigger>(async context =>
        {
            var root = ((BaseContext)context).Context;
            var payload = new EventContext<TEvent>(root).Event;
            var typedContext = new StateMachineInitializationContext<TEvent>(root, new EventHolder<TEvent>
            {
                Payload = payload, Id = root.EventHolder.Id, Headers = root.EventHolder.Headers
            });
            var instance = await binding.ConfigureAsync(root.Executor.GetInitializerAsync<T, TEvent>(typedContext));
            return await instance.OnInitializeAsync(payload);
        });

    private static void AddFinalizer<T>(ClrBinding binding, StateMachineBuilder builder)
        where T : class, IFinalizer
        => builder.AddFinalizer(async context =>
        {
            var instance = await binding.ConfigureAsync(((BaseContext)context).Context.Executor.GetFinalizerAsync<T>(context));
            await instance.OnFinalizeAsync();
        });

    internal static void AddInfrastructure(ClrBinding binding, string role, StateMachineBuilder builder)
    {
        async Task<object> Create(IServiceProvider provider, IStateMachineActionContext context)
        {
            ContextValues.GlobalValuesHolder.Value = context.Behavior.Values;
            ContextValues.StateValuesHolder.Value = null!;
            ContextValues.ParentStateValuesHolder.Value = null!;
            ContextValues.SourceStateValuesHolder.Value = null!;
            ContextValues.TargetStateValuesHolder.Value = null!;
            StateMachinesContextHolder.StateContext.Value = null!;
            StateMachinesContextHolder.TransitionContext.Value = null!;
            StateMachinesContextHolder.BehaviorContext.Value = context.Behavior;
            StateMachinesContextHolder.ParentBehaviorContext.Value = (context.TryGetParentBehaviorContext(out var parent) ? parent : null)!;
            StateMachinesContextHolder.OwnerBehaviorContext.Value = (context.TryGetOwnerBehaviorContext(out var owner) ? owner : null)!;
            StateMachinesContextHolder.StateMachineContext.Value = ((BaseContext)context).StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;
            return await binding.ConfigureAsync(StateflowsActivator.CreateModelElementInstanceAsync(provider, binding.Type, role));
        }
        switch (role)
        {
            case "observers":
                builder.AddObserver(async (provider, context) => (IStateMachineObserver)await Create(provider, context));
                break;
            case "interceptors":
                builder.AddInterceptor(async (provider, context) => (IStateMachineInterceptor)await Create(provider, context));
                break;
            default:
                builder.AddExceptionHandler(async (provider, context) => (IStateMachineExceptionHandler)await Create(provider, context));
                break;
        }
    }
}
