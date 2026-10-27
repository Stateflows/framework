using Stateflows.Common;

namespace Stateflows.MAF.AIAgents
{
    /// <summary>Locates registered Microsoft Agent Framework behaviors through the Stateflows behavior locator.</summary>
    public interface IAIAgentLocator
    {
        /// <summary>Gets the underlying behavior locator used to find agent instances.</summary>
        IBehaviorLocator Locator { get; }

        /// <summary>Attempts to locate the agent behavior identified by its agent identifier.</summary>
        /// <param name="id">The agent type and instance identifier.</param>
        /// <param name="activity">Receives the located agent behavior when one is available.</param>
        /// <returns><see langword="true"/> when the agent behavior was located; otherwise, <see langword="false"/>.</returns>
        bool TryLocateAIAgent(AIAgentId id, out IAIAgentBehavior activity);
    }
}
