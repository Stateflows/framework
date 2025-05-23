namespace Stateflows.StateMachines
{
    public interface IStateActionContext : IStateMachineActionContext
    {
        string ActionName { get; }

        IStateContext State { get; }
    }
}
