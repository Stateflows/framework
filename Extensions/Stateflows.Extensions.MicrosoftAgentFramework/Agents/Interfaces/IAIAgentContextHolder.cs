using Stateflows.Common;

namespace Stateflows.MAF.AIAgents
{
    /// <summary>Owns an asynchronous agent context scope and identifies the associated action behavior.</summary>
    public interface IAIAgentContextHolder : IAsyncDisposable
    {
        /// <summary>Gets the identifier of the agent action using this context.</summary>
        ActionId ActionId { get; }
        /// <summary>Gets the current behavior status of the agent action.</summary>
        BehaviorStatus BehaviorStatus { get; }
        /// <summary>Gets the behavior context exposed to the agent.</summary>
        /// <returns>The agent's Stateflows behavior context.</returns>
        IBehaviorContext GetAgentContext();
    }
}