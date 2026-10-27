namespace Stateflows.MAF.AIAgents
{
    /// <summary>Provides a disposable execution context for an identified AI agent behavior.</summary>
    public interface IAIAgentContextProvider
    {
        /// <summary>Attempts to create or retrieve a context holder for an agent identifier.</summary>
        /// <param name="aiAgentId">The agent behavior instance whose context is requested.</param>
        /// <returns>A task containing whether a context was supplied and its holder when successful.</returns>
        Task<(bool Success, IAIAgentContextHolder ContextHolder)> TryProvideAsync(AIAgentId aiAgentId);
    }
}