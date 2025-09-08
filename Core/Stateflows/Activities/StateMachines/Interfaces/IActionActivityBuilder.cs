namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface IActionActivityBuilder :
        IStateActionInstantation<IInstantiatedActionActivityBuilder>,
        IStateActionInitialization//<IInitializedActionActivityBuilder>,
        //ISubscription<IInitializedActionActivityBuilder>
    { }

    public interface IInstantiatedActionActivityBuilder :
        IStateActionInitialization//<IInitializedActionActivityBuilder>,
        //ISubscription<IInitializedActionActivityBuilder>
    { }

    // public interface IInitializedActionActivityBuilder :
    //     //ISubscription<IInitializedActionActivityBuilder>
    // { }
}
