namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface IActionActionBuilder :
        ISubscription<IActionActionBuilder>
    { }

    public interface IInitializedActionActionBuilder :
        ISubscription<IInitializedActionActionBuilder>
    { }
}
