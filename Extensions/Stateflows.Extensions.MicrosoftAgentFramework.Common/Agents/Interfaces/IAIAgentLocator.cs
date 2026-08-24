using Stateflows.Common;

namespace Stateflows.MAF.AIAgents
{
    public interface IAIAgentLocator
    {
        IBehaviorLocator Locator { get; }

        bool TryLocateAIAgent(AIAgentId id, out IAIAgentBehavior activity);
    }
}
