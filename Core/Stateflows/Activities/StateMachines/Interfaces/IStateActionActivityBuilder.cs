namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface IStateActionActivityBuilder :
        IStateActionInitialization<IInitializedStateActionActivityBuilder>,
        IStateSubscription<IStateActionActivityBuilder>
    { }

    public interface IInitializedStateActionActivityBuilder :
        IStateSubscription<IInitializedStateActionActivityBuilder>
    { }
}
