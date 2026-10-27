using Stateflows.Common;
using Stateflows.Common.Entities.Classes;

namespace Stateflows.Entities
{
    /// <summary>Provides entity-specific lookup from the general behavior locator.</summary>
    public static class IBehaviorLocatorEntityExtensions
    {
        /// <summary>Attempts to locate an entity behavior handle by entity identifier.</summary>
        /// <param name="locator">The behavior locator to query.</param>
        /// <param name="id">The identifier of the entity instance.</param>
        /// <param name="stateMachine">Receives the entity handle when found.</param>
        /// <returns><see langword="true"/> when the entity can be located; otherwise, <see langword="false"/>.</returns>
        public static bool TryLocateEntity(this IBehaviorLocator locator, EntityId id, out IEntityBehavior stateMachine)
        {
            stateMachine = locator.TryLocateBehavior(id, out var behavior)
                ? new EntityWrapper(behavior)
                : null;

            return stateMachine != null;
        }
    }
}
