using Stateflows.Common.Interfaces;

namespace Stateflows.Common
{
    /// <summary>Provides messaging and entity operations for the behavior that owns the current behavior.</summary>
    public interface IOwnerBehaviorContext : 
        ISends<IOwnerBehaviorContext>,
        IPublishes<IOwnerBehaviorContext>,
        ISubscriptions<IOwnerBehaviorContext>,
        IEntityOperations<IOwnerBehaviorContext>
    {
        /// <summary>
        /// Identifies the owning behavior instance.
        /// </summary>
        BehaviorId Id { get; }
    }
}
