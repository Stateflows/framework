using Stateflows.Common;
using Stateflows.Common.StateMachines.Classes;

namespace Stateflows.StateMachines
{
    public static class IBehaviorLocatorStateMachineExtensions
    {
        public static bool TryLocateStateMachine(this IBehaviorLocator locator, StateMachineId id, out IStateMachineBehavior stateMachine)
        {
            stateMachine = locator.TryLocateBehavior(id, out var behavior)
                ? new StateMachineWrapper(behavior)
                : null;

            return stateMachine != null;
        }
    }
}
