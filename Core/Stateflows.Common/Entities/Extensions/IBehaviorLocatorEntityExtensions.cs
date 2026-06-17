using Stateflows.Common;
using Stateflows.Common.Entities.Classes;

namespace Stateflows.Entities
{
    public static class IBehaviorLocatorEntityExtensions
    {
        public static bool TryLocateEntity(this IBehaviorLocator locator, EntityId id, out IEntityBehavior stateMachine)
        {
            stateMachine = locator.TryLocateBehavior(id, out var behavior)
                ? new EntityWrapper(behavior)
                : null;

            return stateMachine != null;
        }
    }
}
