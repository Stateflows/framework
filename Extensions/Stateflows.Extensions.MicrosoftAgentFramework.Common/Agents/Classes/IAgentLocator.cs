using Stateflows.Common;

namespace Stateflows.MAF.AIAgents.Classes
{
    internal class AgentLocator(IBehaviorLocator locator) : IAIAgentLocator
    {
        public IBehaviorLocator Locator { get; } = locator;

        public bool TryLocateAgent(AgentId id, out IAIAgentBehavior agent)
            => Locator.TryLocateAgent(id, out agent);
    }
}
