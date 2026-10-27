using Stateflows.Common;

namespace Stateflows.Entities
{
    /// <summary>Describes entity status and metadata using an entity-specific identifier.</summary>
    [Retain, StrictOwnership]
    public class EntityInfo : BehaviorInfo
    {
        private EntityId id;

        /// <summary>Gets or sets the identifier of the described entity instance.</summary>
        public new EntityId Id
        {
            get => id;
            set
            {
                id = value;
                base.Id = value;
            }
        }
    }
}
