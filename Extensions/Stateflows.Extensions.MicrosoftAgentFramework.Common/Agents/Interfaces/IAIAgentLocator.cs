using Stateflows.Common;

namespace Stateflows.MAF.AIAgents
{
    public interface IAIAgentLocator
    {
        IBehaviorLocator Locator { get; }

        bool TryLocateAgent(AgentId id, out IAIAgentBehavior activity);
    }
}
