using Stateflows.Common.Interfaces;
using Stateflows.Common.StateMachines.Classes;

namespace Stateflows.StateMachines
{
    public static class IBehaviorLocatorExtensions
    {
        public static bool TryLocateStateMachine(this IBehaviorLocator locator, StateMachineId id, out IStateMachine stateMachine)
            => (
                stateMachine = locator.TryLocateBehavior(id.BehaviorId, out var behavior)
                    ? new StateMachineWrapper(behavior)
                    : null
            ) != null;
    }
}
