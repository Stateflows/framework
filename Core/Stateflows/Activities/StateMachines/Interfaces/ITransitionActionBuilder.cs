namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface ITransitionActionBuilder<TEvent> :
        ITransitionInstantation<TEvent>
        //ISubscription<IInstantiatedTransitionActionBuilder<TEvent>>
    { }

    // public interface IInstantiatedTransitionActionBuilder<TEvent> :
    //     //ISubscription<IInstantiatedTransitionActionBuilder<TEvent>>
    // { }
}
