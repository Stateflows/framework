using System;
using System.Threading.Tasks;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Registers machine definitions and global middleware during Stateflows configuration.</summary>
    /// <param name="builder">The state-machine registration builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machines" />
    public delegate void StateMachinesBuildAction(IStateMachinesBuilder builder);

    /// <summary>Defines a machine's states, transitions, lifecycle, and local middleware.</summary>
    /// <param name="builder">The builder scoped to the registered machine definition.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machines" />
    public delegate void StateMachineBuildAction(IStateMachineBuilder builder);
    
    /// <summary>Configures utilities and middleware on a machine whose structure is defined elsewhere.</summary>
    /// <param name="builder">The machine-local utilities builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machines" />
    public delegate void StateMachineUtilsBuildAction(IStateMachineUtilsBuilder builder);
    
    /// <summary>Extends a reused machine definition through the override builder.</summary>
    /// <param name="elementsBuilder">The builder exposing inherited elements and allowed additions.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machines" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenStateMachineBuildAction(IOverridenStateMachineBuilder elementsBuilder);

    /// <summary>Configures a regular state's lifecycle actions, outgoing transitions, and deferrals.</summary>
    /// <param name="builder">The builder scoped to the state.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
    public delegate void StateBuildAction(IStateBuilder builder);
    
    /// <summary>Configures a state that hosts an embedded behavior.</summary>
    /// <param name="builder">The embedded-behavior state's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
    public delegate void BehaviorStateBuildAction(IBehaviorStateBuilder builder);
    
    /// <summary>Extends a regular state inherited from a reused definition.</summary>
    /// <param name="builder">The builder scoped to the inherited state.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenStateBuildAction(IOverridenStateBuilder builder);
    
    /// <summary>Configures the guard controlling whether a state defers an event.</summary>
    /// <typeparam name="TEvent">Deferred event payload type.</typeparam>
    /// <param name="builder">The builder for this deferral rule.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
    public delegate void DeferralBuildAction<TEvent>(IEventDeferralBuilder<TEvent> builder);
    
    /// <summary>Extends an inherited event-deferral rule.</summary>
    /// <typeparam name="TEvent">Deferred event payload type.</typeparam>
    /// <param name="builder">The inherited deferral's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenDeferralBuildAction<TEvent>(IOverridenEventDeferralBuilder<TEvent> builder);

    /// <summary>Configures outgoing branches from a junction pseudostate.</summary>
    /// <param name="builder">The junction's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-junction" />
    public delegate void JunctionBuildAction(IJunctionBuilder builder);
    
    /// <summary>Extends an inherited junction's outgoing branches.</summary>
    /// <param name="builder">The inherited junction's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-junction" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenJunctionBuildAction(IOverridenJunctionBuilder builder);

    /// <summary>Configures outgoing branches from a dynamically evaluated choice pseudostate.</summary>
    /// <param name="builder">The choice's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-choice" />
    public delegate void ChoiceBuildAction(IChoiceBuilder builder);
    
    /// <summary>Extends an inherited choice's outgoing branches.</summary>
    /// <param name="builder">The inherited choice's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-choice" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenChoiceBuildAction(IOverridenChoiceBuilder builder);

    /// <summary>Configures the targets activated by a fork pseudostate.</summary>
    /// <param name="builder">The fork's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-fork" />
    public delegate void ForkBuildAction(IForkBuilder builder);
    
    /// <summary>Extends the targets of an inherited fork.</summary>
    /// <param name="builder">The inherited fork's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-fork" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenForkBuildAction(IOverridenForkBuilder builder);
    
    /// <summary>Configures the continuation of a join that synchronizes incoming regional paths.</summary>
    /// <param name="builder">The join's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-join" />
    public delegate void JoinBuildAction(IJoinBuilder builder);
    
    /// <summary>Extends an inherited join's continuation.</summary>
    /// <param name="builder">The inherited join's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-join" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenJoinBuildAction(IOverridenJoinBuilder builder);
    
    /// <summary>Configures fallback paths used when history has no remembered configuration.</summary>
    /// <param name="builder">The history pseudostate's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-history" />
    public delegate void HistoryBuildAction(IHistoryBuilder builder);

    /// <summary>Defines a composite state's lifecycle and nested region.</summary>
    /// <param name="builder">The composite-state builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
    public delegate void CompositeStateBuildAction(ICompositeStateBuilder builder);

    /// <summary>Extends an inherited composite state and its nested elements.</summary>
    /// <param name="builder">The inherited composite-state builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenCompositeStateBuildAction(IOverridenCompositeStateBuilder builder);

    /// <summary>Defines an orthogonal state's lifecycle and independently active regions.</summary>
    /// <param name="builder">The orthogonal-state builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
    public delegate void OrthogonalStateBuildAction(IOrthogonalStateBuilder builder);

    /// <summary>Extends an inherited orthogonal state and its regions.</summary>
    /// <param name="builder">The inherited orthogonal-state builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenOrthogonalStateBuildAction(IOverridenOrthogonalStateBuilder builder);

    /// <summary>Defines one region's initial state, vertices, and final state.</summary>
    /// <param name="builder">The region-scoped builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
    public delegate void RegionBuildAction(IRegionBuilder builder);

    /// <summary>Extends the elements of an inherited region.</summary>
    /// <param name="builder">The inherited region's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenRegionBuildAction(IOverridenRegionBuilder builder);

    /// <summary>Configures a triggered transition's guards and effects.</summary>
    /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
    /// ordinary event triggers use exact matching.</typeparam>
    /// <param name="builder">The transition-scoped builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
    public delegate void TransitionBuildAction<TEvent>(ITransitionBuilder<TEvent> builder);
    
    /// <summary>Extends an inherited triggered transition.</summary>
    /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
    /// ordinary event triggers use exact matching.</typeparam>
    /// <param name="builder">The inherited transition's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenTransitionBuildAction<TEvent>(IOverridenTransitionBuilder<TEvent> builder);

    /// <summary>Configures a transition that consumes an event without exiting or re-entering the source state.
    /// </summary>
    /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
    /// ordinary event triggers use exact matching.</typeparam>
    /// <param name="builder">The internal transition's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-internal-transition" />
    public delegate void InternalTransitionBuildAction<TEvent>(IInternalTransitionBuilder<TEvent> builder);

    /// <summary>Extends an inherited internal transition.</summary>
    /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
    /// ordinary event triggers use exact matching.</typeparam>
    /// <param name="builder">The inherited internal transition's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-internal-transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenInternalTransitionBuildAction<TEvent>(IOverridenInternalTransitionBuilder<TEvent> builder);

    /// <summary>Configures a completion-triggered transition's guard and effect.</summary>
    /// <param name="builder">The default transition's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-default-transition" />
    public delegate void DefaultTransitionBuildAction(IDefaultTransitionBuilder builder);
    
    /// <summary>Extends an inherited completion-triggered transition.</summary>
    /// <param name="builder">The inherited default transition's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-default-transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public delegate void OverridenDefaultTransitionBuildAction(IOverridenDefaultTransitionBuilder builder);

    /// <summary>Configures an effect-only completion path, such as a fork or join continuation.</summary>
    /// <param name="builder">The completion path's effect builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
    public delegate void DefaultTransitionEffectBuildAction(IDefaultTransitionEffectBuilder builder);

    /// <summary>Configures the effect for a fallback transition selected when ordinary transition guards reject their
    /// paths.</summary>
    /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
    /// ordinary event triggers use exact matching.</typeparam>
    /// <param name="builder">The fallback transition's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
    public delegate void ElseTransitionBuildAction<TEvent>(IElseTransitionBuilder<TEvent> builder);

    /// <summary>Configures an inherited fallback transition.</summary>
    /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
    /// ordinary event triggers use exact matching.</typeparam>
    /// <param name="builder">The fallback transition's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
    public delegate void OverridenElseTransitionBuildAction<TEvent>(IElseTransitionBuilder<TEvent> builder);

    /// <summary>Configures a fallback internal transition that preserves the active state configuration.</summary>
    /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
    /// ordinary event triggers use exact matching.</typeparam>
    /// <param name="builder">The fallback internal transition's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-internal-transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
    public delegate void ElseInternalTransitionBuildAction<TEvent>(IElseInternalTransitionBuilder<TEvent> builder);

    /// <summary>Configures an inherited fallback internal transition.</summary>
    /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
    /// ordinary event triggers use exact matching.</typeparam>
    /// <param name="builder">The inherited transition's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-internal-transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
    public delegate void OverridenElseInternalTransitionBuildAction<TEvent>(IElseInternalTransitionBuilder<TEvent> builder);

    /// <summary>Configures the fallback completion transition used when other completion guards reject their paths.
    /// </summary>
    /// <param name="builder">The fallback completion transition's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-default-transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
    public delegate void ElseDefaultTransitionBuildAction(IElseDefaultTransitionBuilder builder);

    /// <summary>Configures an inherited fallback completion transition.</summary>
    /// <param name="builder">The fallback completion transition's builder.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-default-transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
    public delegate void OverridenElseDefaultTransitionBuildAction(IElseDefaultTransitionBuilder builder);

    /// <summary>Creates an observer for a machine execution scope.</summary>
    /// <param name="serviceProvider">The execution's scoped service provider.</param>
    /// <param name="context">The machine action context.</param>
    /// <returns>The observer for the machine execution scope.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
    public delegate IStateMachineObserver StateMachineObserverFactory(IServiceProvider serviceProvider, IStateMachineActionContext context);
    /// <summary>Asynchronously creates an observer for a machine execution scope.</summary>
    /// <param name="serviceProvider">The execution's scoped service provider.</param>
    /// <param name="context">The machine action context.</param>
    /// <returns>A task producing the observer.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
    public delegate Task<IStateMachineObserver> StateMachineObserverFactoryAsync(IServiceProvider serviceProvider, IStateMachineActionContext context);

    /// <summary>Creates event-processing middleware for a machine execution scope.</summary>
    /// <param name="serviceProvider">The execution's scoped service provider.</param>
    /// <param name="context">The machine action context.</param>
    /// <returns>The interceptor, which may short-circuit processing or replace its status.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
    public delegate IStateMachineInterceptor StateMachineInterceptorFactory(IServiceProvider serviceProvider, IStateMachineActionContext context);
    /// <summary>Asynchronously creates event-processing middleware for a machine execution scope.</summary>
    /// <param name="serviceProvider">The execution's scoped service provider.</param>
    /// <param name="context">The machine action context.</param>
    /// <returns>A task producing the interceptor.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
    public delegate Task<IStateMachineInterceptor> StateMachineInterceptorFactoryAsync(IServiceProvider serviceProvider, IStateMachineActionContext context);

    /// <summary>Creates a failure acknowledgement handler for a machine execution scope.</summary>
    /// <param name="serviceProvider">The execution's scoped service provider.</param>
    /// <param name="context">The machine action context.</param>
    /// <returns>The handler; acknowledgement does not recover failed execution.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
    public delegate IStateMachineExceptionHandler StateMachineExceptionHandlerFactory(IServiceProvider serviceProvider, IStateMachineActionContext context);
    /// <summary>Asynchronously creates a failure acknowledgement handler.</summary>
    /// <param name="serviceProvider">The execution's scoped service provider.</param>
    /// <param name="context">The machine action context.</param>
    /// <returns>A task producing the exception handler.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
    public delegate Task<IStateMachineExceptionHandler> StateMachineExceptionHandlerFactoryAsync(IServiceProvider serviceProvider, IStateMachineActionContext context);
    
    /// <summary>Builds an initialization payload for a behavior invoked from a state action.</summary>
    /// <param name="context">The invoking state's action context.</param>
    /// <returns>The initialization payload to send to the invoked behavior.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
    public delegate object StateActionInitializationBuilder(IStateActionContext context);
}