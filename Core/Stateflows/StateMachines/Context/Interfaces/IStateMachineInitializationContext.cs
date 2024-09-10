namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IStateMachineInitializationContext : IStateMachineActionContext
    { }

    public interface IStateMachineInitializationContext<out TInitializationEvent> : IStateMachineInitializationContext
    {
        TInitializationEvent InitializationEvent { get; }
    }
}
