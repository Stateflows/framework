namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface ITransitionActivityBuilder<TEvent> :
        ITransitionInitialization<TEvent>,
        ITransitionInstantation<TEvent, IInstantiatedTransitionActivityBuilder<TEvent>>
        //ISubscription<IInitializedTransitionActivityBuilder<TEvent>>
    { }

    public interface IInstantiatedTransitionActivityBuilder<TEvent> :
        ITransitionInitialization<TEvent>
        //ISubscription<IInitializedTransitionActivityBuilder<TEvent>>
    { }

    // public interface IInitializedTransitionActivityBuilder<TEvent> :
    //     ISubscription<IInitializedTransitionActivityBuilder<TEvent>>
    // { }
}
