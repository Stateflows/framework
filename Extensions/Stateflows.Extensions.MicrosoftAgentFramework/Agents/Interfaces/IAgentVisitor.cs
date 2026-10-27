using Stateflows.Common;

namespace Stateflows.MAF.AIAgents
{
    /// <summary>Observes the registration of agent actions and their custom event definitions.</summary>
    public interface IAgentVisitor
    {
        /// <summary>Notifies the visitor that an agent action has been added.</summary>
        /// <param name="actionName">The registered action name.</param>
        /// <param name="actionVersion">The registered action version.</param>
        /// <returns>A task that completes after the notification is handled.</returns>
        Task AgentAddedAsync(string actionName, int actionVersion);
        
        /// <summary>Notifies the visitor that an agent action backed by a typed agent has been added.</summary>
        /// <typeparam name="TAction">The registered agent implementation type.</typeparam>
        /// <param name="actionName">The registered action name.</param>
        /// <param name="actionVersion">The registered action version.</param>
        /// <returns>A task that completes after the notification is handled.</returns>
        Task AgentTypeAddedAsync<TAction>(string actionName, int actionVersion)
            where TAction : class, IAIAgent;
        
        /// <summary>Notifies the visitor that a custom event is available to an agent action.</summary>
        /// <typeparam name="TEvent">The custom event type.</typeparam>
        /// <param name="actionName">The agent action name.</param>
        /// <param name="actionVersion">The agent action version.</param>
        /// <param name="supportedStatuses">The behavior statuses for which the event is available.</param>
        /// <returns>A task that completes after the notification is handled.</returns>
        Task CustomEventAddedAsync<TEvent>(string actionName, int actionVersion, BehaviorStatus[] supportedStatuses);

        /// <summary>Notifies the visitor that a custom event definition is available to an agent action.</summary>
        /// <param name="definition">The custom event definition.</param>
        /// <param name="actionName">The agent action name.</param>
        /// <param name="actionVersion">The agent action version.</param>
        /// <param name="supportedStatuses">The behavior statuses for which the event is available.</param>
        /// <returns>A task that completes after the notification is handled.</returns>
        Task CustomEventAddedAsync(EventDefinition definition, string actionName, int actionVersion, BehaviorStatus[] supportedStatuses)
            => Task.CompletedTask;
    }
}
