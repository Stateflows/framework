using System;
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

    public delegate void TransitionBuildAction<TEvent>(ITransitionBuilder<TEvent> builder)
        where TEvent : Event, new();

    public delegate void ElseTransitionBuildAction<TEvent>(IElseTransitionBuilder<TEvent> builder)
        where TEvent : Event, new();

    public delegate IStateMachineObserver StateMachineObserverFactory(IServiceProvider serviceProvider);

    public delegate IStateMachineInterceptor StateMachineInterceptorFactory(IServiceProvider serviceProvider);

    public delegate IStateMachineExceptionHandler StateMachineExceptionHandlerFactory(IServiceProvider serviceProvider);

    public delegate InitializationRequest StateActionInitializationBuilder(IStateActionContext context);
}