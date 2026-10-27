using System;

namespace Stateflows.StateMachines
{
    /// <summary>Describes the source, optional target, and triggering event of the current transition.</summary>
    /// <remarks>See <see href="https://github.com/Stateflows/framework/wiki/state-machine-transition">transition semantics</see>.</remarks>
    public interface ITransitionContext
    {
        /// <summary>The state from which the transition originates.</summary>
        IStateContext Source { get; }

        /// <summary>The destination state, or <see langword="null"/> for an internal transition.</summary>
        IStateContext? Target { get; }
        
        /// <summary>The runtime trigger type used by this transition.</summary>
        Type TriggerType { get; }
        
        /// <summary>The triggering event object.</summary>
        object Trigger { get; }
    }
    
    /// <summary>Provides a transition trigger with a statically known event type.</summary>
    /// <typeparam name="TEvent">The transition's event type.</typeparam>
    public interface ITransitionContextX<TEvent> : ITransitionContext
    {
        /// <inheritdoc/>
        IStateContext Source { get; }

        /// <inheritdoc/>
        IStateContext Target { get; }
        
        /// <inheritdoc/>
        Type TriggerType { get; }

        /// <summary>The triggering event cast to the configured event type.</summary>
        new TEvent Trigger { get; }
    }
}
