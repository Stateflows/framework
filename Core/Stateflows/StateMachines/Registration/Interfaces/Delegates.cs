using System;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public delegate void StateMachinesBuilderAction(IStateMachinesBuilder builder);

    public delegate void StateMachineBuilderAction(IStateMachineBuilder builder);

    public delegate void StateBuilderAction(IStateBuilder builder);

    public delegate void StateTransitionsBuilderAction(ITypedStateBuilder builder);

    public delegate void CompositeStateBuilderAction(ICompositeStateBuilder builder);

    public delegate void CompositeStateTransitionsBuilderAction(ITypedCompositeStateBuilder builder);

    public delegate void TransitionBuilderAction<TEvent>(ITransitionBuilder<TEvent> builder)
        where TEvent : Event, new();

    public delegate IStateMachineObserver StateMachineObserverFactory(IServiceProvider serviceProvider);

    public delegate IStateMachineInterceptor StateMachineInterceptorFactory(IServiceProvider serviceProvider);

    public delegate IStateMachineExceptionHandler StateMachineExceptionHandlerFactory(IServiceProvider serviceProvider);

    public delegate InitializationRequest StateActionInitializationBuilder(IStateActionContext context);
}