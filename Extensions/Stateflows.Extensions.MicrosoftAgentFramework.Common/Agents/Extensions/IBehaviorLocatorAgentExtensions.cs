using Stateflows.Common;
using Stateflows.MAF.AIAgents;
using Stateflows.MAF.AIAgents.Classes;

namespace Stateflows
{
    public static class IBehaviorLocatorAgentExtensions
    {
        public static bool TryLocateAgent(this IBehaviorLocator locator, AgentId id, out IAIAgentBehavior action)
            => (
                action = locator.TryLocateBehavior(id.BehaviorId, out var behavior)
                    ? new AgentWrapper(behavior)
                    : null
            ) != null;
    }
}
