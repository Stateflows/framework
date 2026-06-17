using Stateflows.Common;

namespace Stateflows.Entities
{
    [Retain, StrictOwnership]
    public class EntityInfo : BehaviorInfo
    {
        private EntityId id;

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
