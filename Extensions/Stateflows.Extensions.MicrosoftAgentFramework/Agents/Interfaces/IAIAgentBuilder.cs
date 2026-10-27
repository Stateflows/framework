using Microsoft.Extensions.AI;
using Stateflows.Common;

namespace Stateflows.MAF.AIAgents;

/// <summary>Configures agent metadata, prompts, and event/token formatting for a registered agent action.</summary>
public interface IAIAgentBuilder
{
    /// <summary>Sets the name supplied to the framework agent configuration.</summary>
    /// <param name="name">The agent name.</param>
    /// <returns>This builder for chaining.</returns>
    IAIAgentBuilder SetName(string name);
    /// <summary>Sets the description supplied to the framework agent configuration.</summary>
    /// <param name="description">The agent description.</param>
    /// <returns>This builder for chaining.</returns>
    IAIAgentBuilder SetDescription(string description);
    /// <summary>Sets the initial prompt supplied to the framework agent.</summary>
    /// <param name="initialPrompt">The prompt text.</param>
    /// <returns>This builder for chaining.</returns>
    IAIAgentBuilder SetInitialPrompt(string initialPrompt);
    /// <summary>Registers a formatter for consumed events of a CLR event type.</summary>
    /// <typeparam name="TEvent">The event type consumed by the agent.</typeparam>
    /// <param name="eventFormatterAsync">Asynchronously converts the event and agent context to a chat message.</param>
    /// <returns>This builder for chaining.</returns>
    IAIAgentBuilder AddConsumedEvent<TEvent>(Func<IAIAgentContext, TEvent, Task<ChatMessage>> eventFormatterAsync);
    /// <summary>Registers a formatter for a consumed structural event name.</summary>
    /// <param name="eventName">The structural event name.</param>
    /// <param name="eventFormatterAsync">Asynchronously converts the structural event and agent context to a chat message.</param>
    /// <returns>This builder for chaining.</returns>
    IAIAgentBuilder AddConsumedEvent(string eventName, Func<IAIAgentContext, StructuralEvent, Task<ChatMessage>> eventFormatterAsync);
    /// <summary>Registers a formatter for consumed tokens of a CLR token type.</summary>
    /// <typeparam name="TToken">The token type consumed by the agent.</typeparam>
    /// <param name="tokenFormatterAsync">Asynchronously converts the token and agent context to a chat message.</param>
    /// <returns>This builder for chaining.</returns>
    IAIAgentBuilder AddConsumedToken<TToken>(Func<IAIAgentContext, TToken, Task<ChatMessage>> tokenFormatterAsync);
}