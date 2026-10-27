using Stateflows.Common.Interfaces;

namespace Stateflows.Common
{
    /// <summary>Provides messaging and entity operations for the parent containing the current behavior.</summary>
    public interface IParentBehaviorContext :
        ISends<IParentBehaviorContext>,
        IPublishes<IParentBehaviorContext>,
        ISubscriptions<IParentBehaviorContext>,
        IEntityOperations<IParentBehaviorContext>
    {
        /// <summary>
        /// Identifies the parent behavior instance.
        /// </summary>
        BehaviorId Id { get; }
    }
}
