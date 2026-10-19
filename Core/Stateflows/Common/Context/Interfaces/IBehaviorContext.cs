using Stateflows.Common.Interfaces;

namespace Stateflows.Common
{
    public interface IBehaviorContext :
        ISends<IBehaviorContext>,
        IPublishes<IBehaviorContext>,
        ISubscriptions<IBehaviorContext>,
        IInjectionScope,
        IEntityOperations<IBehaviorContext>
    {
        /// <summary>
        /// Represents identifier of current behavior
        /// </summary>
        BehaviorId Id { get; }

        /// <summary>
        /// Provides access to global context values of current behavior
        /// </summary>
        IContextValues Values { get; }
        
        bool IsEmbedded { get; }
    }
}
