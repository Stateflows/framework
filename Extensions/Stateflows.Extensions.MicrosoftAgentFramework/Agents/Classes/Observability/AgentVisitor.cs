using Stateflows.Common;

namespace Stateflows.MAF.AIAgents
{
    public abstract class AgentVisitor : IAgentVisitor
    {
        public virtual Task AgentAddedAsync(string actionName, int actionVersion)
            => Task.CompletedTask;

        public virtual Task AgentTypeAddedAsync<TAction>(string actionName, int actionVersion) where TAction : class, IAIAgent
            => Task.CompletedTask;

        public virtual Task CustomEventAddedAsync<TEvent>(string actionName, int actionVersion, BehaviorStatus[] supportedStatuses)
            => Task.CompletedTask;
    }
}