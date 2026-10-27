using Stateflows.Common;

namespace Stateflows.MAF.AIAgents
{
    /// <summary>Intercepts event processing by a registered AI agent action.</summary>
    public interface IAIAgentInterceptor
    {
        /// <summary>Runs before the agent action processes an event.</summary>
        /// <typeparam name="TEvent">The event type being processed.</typeparam>
        /// <param name="context">The event context for the agent action.</param>
        /// <returns><see langword="true"/> to continue processing or <see langword="false"/> to stop it.</returns>
        bool BeforeProcessEvent<TEvent>(Actions.Context.Interfaces.IEventContext<TEvent> context);

        /// <summary>Runs after the agent action finishes processing an event.</summary>
        /// <typeparam name="TEvent">The event type that was processed.</typeparam>
        /// <param name="context">The event context for the agent action.</param>
        /// <param name="eventStatus">The event processing result.</param>
        void AfterProcessEvent<TEvent>(Actions.Context.Interfaces.IEventContext<TEvent> context, EventStatus eventStatus);

    }
}
