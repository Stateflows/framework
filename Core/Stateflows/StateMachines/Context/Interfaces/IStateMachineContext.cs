using Stateflows.Common.Context.Interfaces;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IStateMachineContext : IBehaviorContext
    {
        new StateMachineId Id { get; }
    }
}
