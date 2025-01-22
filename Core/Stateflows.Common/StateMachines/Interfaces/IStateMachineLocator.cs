namespace Stateflows.StateMachines
{
    public interface IStateMachineLocator
    {
        public bool TryLocateStateMachine(StateMachineId id, out IStateMachineBehavior stateMachine);
    }
}
