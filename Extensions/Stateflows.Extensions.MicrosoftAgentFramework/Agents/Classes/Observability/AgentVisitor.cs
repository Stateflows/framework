using Stateflows.Common;

namespace Stateflows.MAF.AIAgents
{
    /// <summary>Provides a no-op base implementation for observing agent registrations.</summary>
    public abstract class AgentVisitor : IAgentVisitor
    {
        /// <inheritdoc/>
        public virtual Task AgentAddedAsync(string actionName, int actionVersion)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task AgentTypeAddedAsync<TAction>(string actionName, int actionVersion) where TAction : class, IAIAgent
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task CustomEventAddedAsync<TEvent>(string actionName, int actionVersion, BehaviorStatus[] supportedStatuses)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual Task CustomEventAddedAsync(EventDefinition definition, string actionName, int actionVersion, BehaviorStatus[] supportedStatuses)
            => Task.CompletedTask;
    }
}