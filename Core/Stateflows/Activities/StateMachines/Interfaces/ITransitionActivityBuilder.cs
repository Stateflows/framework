using Stateflows.Common;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface ITransitionActivityBuilder<TEvent> :
        ITransitionInitialization<TEvent, IInitializedTransitionActivityBuilder<TEvent>>,
        ISubscription<ITransitionActivityBuilder<TEvent>>
        where TEvent : Event, new()
    { }

    public interface IInitializedTransitionActivityBuilder<TEvent> :
        ISubscription<IInitializedTransitionActivityBuilder<TEvent>>
        where TEvent : Event, new()
    { }
}
