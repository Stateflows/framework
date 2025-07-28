namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface IActionActivityBuilder :
        IStateActionInitialization<IInitializedActionActivityBuilder>,
        ISubscription<IActionActivityBuilder>
    { }

    public interface IInitializedActionActivityBuilder :
        ISubscription<IInitializedActionActivityBuilder>
    { }
}
