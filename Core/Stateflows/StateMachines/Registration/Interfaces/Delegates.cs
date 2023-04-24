using Stateflows.Common;
using System;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public delegate void StateMachineBuilderAction(IStateMachineInitialBuilder builder);

    public delegate void StateBuilderAction(IStateBuilder builder);

    public delegate void StateTransitionsBuilderAction(ITypedStateBuilder builder);

    public delegate void CompositeStateBuilderAction(ICompositeStateInitialBuilder builder);

    public delegate void CompositeStateTransitionsBuilderAction(ITypedCompositeStateInitialBuilder builder);

    public delegate void TransitionBuilderAction<TEvent>(ITransitionBuilder<TEvent> builder)
        where TEvent : Event, new();

    public delegate IStateMachineObserver ObserverFactory(IServiceProvider serviceProvider);

    public delegate IStateMachineInterceptor InterceptorFactory(IServiceProvider serviceProvider);

    public delegate IStateMachineExceptionHandler ExceptionHandlerFactory(IServiceProvider serviceProvider);
}