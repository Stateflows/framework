using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public delegate void StateMachinesBuildAction(IStateMachinesBuilder builder);

    public delegate void StateMachineBuildAction(IStateMachineBuilder builder);
    
    public delegate void OverridenStateMachineBuildAction(IOverridenStateMachineBuilder builder);

    public delegate void StateBuildAction(IStateBuilder builder);
    
    public delegate void OverridenStateBuildAction(IOverridenStateBuilder builder);

    public delegate void JunctionBuildAction(IJunctionBuilder builder);
    
    public delegate void OverridenJunctionBuildAction(IOverridenJunctionBuilder builder);

    public delegate void ChoiceBuildAction(IChoiceBuilder builder);
    
    public delegate void OverridenChoiceBuildAction(IOverridenChoiceBuilder builder);

    public delegate void StateTransitionsBuildAction(IStateBuilder builder);

    public delegate void CompositeStateBuildAction(ICompositeStateBuilder builder);

    public delegate void OverridenCompositeStateBuildAction(IOverridenCompositeStateBuilder builder);

    public delegate void CompositeStateTransitionsBuildAction(ICompositeStateBuilder builder);

    public delegate void OrthogonalStateBuildAction(IOrthogonalStateBuilder builder);

    public delegate void OverridenOrthogonalStateBuildAction(IOverridenOrthogonalStateBuilder builder);

    public delegate void RegionBuildAction(IRegionBuilder builder);

    public delegate void OverridenRegionBuildAction(IOverridenRegionBuilder builder);

    public delegate void TransitionBuildAction<TEvent>(ITransitionBuilder<TEvent> builder);

    public delegate void InternalTransitionBuildAction<TEvent>(IInternalTransitionBuilder<TEvent> builder);

    public delegate void DefaultTransitionBuildAction(IDefaultTransitionBuilder builder);

    public delegate void ElseTransitionBuildAction<TEvent>(IElseTransitionBuilder<TEvent> builder);

    public delegate void ElseInternalTransitionBuildAction<TEvent>(IElseInternalTransitionBuilder<TEvent> builder);

    public delegate void ElseDefaultTransitionBuildAction(IElseDefaultTransitionBuilder builder);

    public delegate void EmbeddedBehaviorBuildAction(IEmbeddedBehaviorBuilder builder);

    public delegate void ForwardedEventBuildAction<TEvent>(IForwardedEventBuilder<TEvent> builder);

    public delegate IStateMachineObserver StateMachineObserverFactory(IServiceProvider serviceProvider);

    public delegate IStateMachineInterceptor StateMachineInterceptorFactory(IServiceProvider serviceProvider);

    public delegate IStateMachineExceptionHandler StateMachineExceptionHandlerFactory(IServiceProvider serviceProvider);

    public delegate object StateActionInitializationBuilder(IStateActionContext context);
    
    public delegate Task<object> StateActionInitializationBuilderAsync(IStateActionContext context);
}