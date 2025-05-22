namespace Stateflows.StateMachines
{
    public interface IStateMachineInitializationContext : IStateMachineActionContext
    { }

    public interface IStateMachineInitializationContext<out TInitializationEvent> : IStateMachineInitializationContext
    {
        TInitializationEvent InitializationEvent { get; }
    }
}
