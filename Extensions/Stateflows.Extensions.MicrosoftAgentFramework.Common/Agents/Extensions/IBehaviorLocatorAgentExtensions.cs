using Stateflows.Common;
using Stateflows.MAF.AIAgents;
using Stateflows.MAF.AIAgents.Classes;

namespace Stateflows
{
    public static class IBehaviorLocatorAgentExtensions
    {
        public static bool TryLocateAIAgent(this IBehaviorLocator locator, AIAgentId id, out IAIAgentBehavior action)
            => (
                action = locator.TryLocateBehavior(id.BehaviorId, out var behavior)
                    ? new AIAgentWrapper(behavior)
                    : null
            ) != null;
    }
}
