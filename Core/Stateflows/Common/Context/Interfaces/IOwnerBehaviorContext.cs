using Stateflows.Common.Interfaces;

namespace Stateflows.Common
{
    public interface IOwnerBehaviorContext : 
        ISends<IOwnerBehaviorContext>,
        IPublishes<IOwnerBehaviorContext>,
        ISubscriptions<IOwnerBehaviorContext>,
        IEntityOperations<IOwnerBehaviorContext>
    {
        /// <summary>
        /// Represents identifier of owner behavior
        /// </summary>
        BehaviorId Id { get; }
    }
}
