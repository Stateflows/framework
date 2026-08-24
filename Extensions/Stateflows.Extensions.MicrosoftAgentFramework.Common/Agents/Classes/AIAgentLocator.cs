using Stateflows.Common;

namespace Stateflows.MAF.AIAgents.Classes
{
    internal class AIAgentLocator(IBehaviorLocator locator) : IAIAgentLocator
    {
        public IBehaviorLocator Locator { get; } = locator;

        public bool TryLocateAIAgent(AIAgentId id, out IAIAgentBehavior agent)
            => Locator.TryLocateAIAgent(id, out agent);
    }
}
