using Stateflows.Entities;

namespace Stateflows.Common.Entities.Classes
{
    internal class EntityLocator : IEntityLocator
    {
        private IBehaviorLocator Locator { get; }

        public EntityLocator(IBehaviorLocator locator)
        {
            Locator = locator;
        }

        public bool TryLocateEntity(EntityId id, out IEntityBehavior stateMachine)
            => Locator.TryLocateEntity(id, out stateMachine);
    }
}
