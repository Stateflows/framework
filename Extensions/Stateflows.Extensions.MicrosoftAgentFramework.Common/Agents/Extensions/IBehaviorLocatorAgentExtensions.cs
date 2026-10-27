using Stateflows.Common;
using Stateflows.MAF.AIAgents;
using Stateflows.MAF.AIAgents.Classes;

namespace Stateflows
{
    /// <summary>Provides lookup helpers for typed Microsoft Agent Framework behavior identifiers.</summary>
    public static class IBehaviorLocatorAgentExtensions
    {
        /// <summary>Attempts to locate an agent behavior and wrap it with the agent-specific behavior contract.</summary>
        /// <param name="locator">The Stateflows behavior locator.</param>
        /// <param name="id">The agent behavior identifier.</param>
        /// <param name="action">Receives the located agent behavior when available.</param>
        /// <returns><see langword="true"/> when the underlying behavior was located; otherwise, <see langword="false"/>.</returns>
        public static bool TryLocateAIAgent(this IBehaviorLocator locator, AIAgentId id, out IAIAgentBehavior action)
            => (
                action = locator.TryLocateBehavior(id.BehaviorId, out var behavior)
                    ? new AIAgentWrapper(behavior)
                    : null
            ) != null;
    }
}
