using Stateflows.Common;

namespace Stateflows.MAF.AIAgents
{
    public interface IAIAgentContextHolder : IAsyncDisposable
    {
        ActionId ActionId { get; }
        BehaviorStatus BehaviorStatus { get; }
        IBehaviorContext GetAgentContext();
    }
}