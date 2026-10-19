using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.Extensions.MinimalAPIs.Interfaces
{
    public interface IBehaviorContext :
        ISends<IBehaviorContext>,
        ISubscriptions<IBehaviorContext>,
        IInjectionScope,
        IEntityOperations<IBehaviorContext>
    {
        /// <summary>
        /// Represents identifier of current behavior
        /// </summary>
        BehaviorId Id { get; }
        
        bool IsEmbedded { get; }
    }
}
