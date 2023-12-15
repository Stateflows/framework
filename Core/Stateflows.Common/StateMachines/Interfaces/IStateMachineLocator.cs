using System;
using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public interface IStateMachineLocator
    {
        bool TryLocateStateMachine(StateMachineId id, out IStateMachine stateMachine);
    }
}
