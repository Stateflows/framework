namespace Stateflows.MAF.AIAgents
{
    public interface IAIAgentContextProvider
    {
        Task<(bool Success, IAIAgentContextHolder ContextHolder)> TryProvideAsync(AgentId agentId);
    }
}