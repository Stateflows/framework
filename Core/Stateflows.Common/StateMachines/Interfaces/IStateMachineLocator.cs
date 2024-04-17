namespace Stateflows.StateMachines
{
    public interface IStateMachineLocator
    {
        bool TryLocateStateMachine(StateMachineId id, out IStateMachine stateMachine);
    }
}
