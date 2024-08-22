namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface IStateActionActivityBuilder :
        IStateActionInitialization<IInitializedStateActionActivityBuilder>,
        ISubscription<IStateActionActivityBuilder>
    { }

    public interface IInitializedStateActionActivityBuilder :
        ISubscription<IInitializedStateActionActivityBuilder>
    { }
}
