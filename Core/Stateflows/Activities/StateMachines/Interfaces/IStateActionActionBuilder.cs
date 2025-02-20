namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface IStateActionActionBuilder :
        IStateSubscription<IStateActionActionBuilder>
    { }

    public interface IInitializedStateActionActionBuilder :
        IStateSubscription<IInitializedStateActionActionBuilder>
    { }
}
