using Stateflows.Common.Interfaces;

namespace Stateflows.Common
{
    /// <summary>Exposes the current behavior instance's messaging, values, and nested entity operations.</summary>
    /// <remarks>The context is valid only during the behavior execution that supplies it.</remarks>
    public interface IBehaviorContext :
        ISends<IBehaviorContext>,
        IPublishes<IBehaviorContext>,
        ISubscriptions<IBehaviorContext>,
        IInjectionScope,
        IEntityOperations<IBehaviorContext>
    {
        /// <summary>
        /// Identifies the current behavior instance.
        /// </summary>
        BehaviorId Id { get; }

        /// <summary>
        /// Provides access to the current behavior's persistent value store.
        /// </summary>
        IContextValues Values { get; }
        
        /// <summary>Gets whether this behavior is embedded in another behavior.</summary>
        bool IsEmbedded { get; }
    }
}
