using Microsoft.Extensions.AI;

namespace Stateflows.MAF.AIAgents;

/// <summary>Converts a source value into a framework chat message for an agent.</summary>
/// <typeparam name="TSource">The event or token type to format.</typeparam>
public interface IChatMessageConverter<in TSource>
{
    /// <summary>Converts one source value using the current agent context.</summary>
    /// <param name="source">The event or token value to convert.</param>
    /// <param name="context">The current agent execution context.</param>
    /// <param name="cancellationToken">A token that can cancel the conversion.</param>
    /// <returns>A task containing the resulting chat message.</returns>
    Task<ChatMessage> ConvertAsync(TSource source, IAIAgentContext context, CancellationToken cancellationToken = default);
}