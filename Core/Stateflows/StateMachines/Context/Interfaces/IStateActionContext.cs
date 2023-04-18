using Stateflows.Common.Interfaces;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IStateActionContext : IStateMachineActionContext
    {
        string ActionName { get; }

        IStateContext CurrentState { get; }
    }
}
