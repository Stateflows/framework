using Stateflows.Common.Interfaces;

namespace Stateflows.Common
{
    public interface IParentBehaviorContext :
        ISends<IParentBehaviorContext>,
        IPublishes<IParentBehaviorContext>,
        ISubscriptions<IParentBehaviorContext>,
        IEntityOperations<IParentBehaviorContext>
    {
        /// <summary>
        /// Represents identifier of parent behavior
        /// </summary>
        BehaviorId Id { get; }
    }
}
