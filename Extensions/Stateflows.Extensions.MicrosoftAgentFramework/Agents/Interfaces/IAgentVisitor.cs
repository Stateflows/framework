using Stateflows.Common;

namespace Stateflows.MAF.AIAgents
{
    public interface IAgentVisitor
    {
        Task AgentAddedAsync(string actionName, int actionVersion);
        
        Task AgentTypeAddedAsync<TAction>(string actionName, int actionVersion)
            where TAction : class, IAIAgent;
        
        Task CustomEventAddedAsync<TEvent>(string actionName, int actionVersion, BehaviorStatus[] supportedStatuses);
    }
}
