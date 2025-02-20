using Stateflows.Common;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface ITransitionActionBuilder<TEvent> :
        ISubscription<ITransitionActionBuilder<TEvent>>

    { }

    public interface IInitializedTransitionActionBuilder<TEvent> :
        ISubscription<IInitializedTransitionActionBuilder<TEvent>>

    { }
}
