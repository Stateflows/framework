using System;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;
using Stateflows.Common.StateMachines.Classes;

namespace Stateflows.StateMachines
{
    public static class IBehaviorLocatorStateMachineExtensions
    {
        public static bool TryLocateStateMachine(this IBehaviorLocator locator, StateMachineId id, out IStateMachine stateMachine)
        {
            stateMachine = locator.TryLocateBehavior(id.BehaviorId, out var behavior)
                ? new StateMachineWrapper(behavior)
                : null;

            return stateMachine != null;
        }
    }
}
