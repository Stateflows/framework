using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.Extensions.MinimalAPIs.Interfaces
{
    /// <summary>Provides behavior identity, messaging, subscriptions, Entity access, and dependency injection in a custom endpoint.</summary>
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
        
        /// <summary>Gets whether the addressed behavior is embedded in another behavior.</summary>
        bool IsEmbedded { get; }
    }
}
