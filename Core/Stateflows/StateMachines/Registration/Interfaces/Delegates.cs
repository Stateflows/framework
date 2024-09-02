using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public delegate void StateMachinesBuildAction(IStateMachinesBuilder builder);

    public delegate void StateMachineBuildAction(IStateMachineBuilder builder);

    public delegate void StateBuildAction(IStateBuilder builder);

    public delegate void StateTransitionsBuildAction(ITypedStateBuilder builder);

    public delegate void CompositeStateBuildAction(ICompositeStateBuilder builder);

    public delegate void CompositeStateTransitionsBuildAction(ITypedCompositeStateBuilder builder);

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

    public delegate Event StateActionInitializationBuilder(IStateActionContext context);
    
    public delegate Task<Event> StateActionInitializationBuilderAsync(IStateActionContext context);
}