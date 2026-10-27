using Microsoft.Extensions.AI;
using Stateflows.Common;
using Stateflows.MAF.AIAgents;

namespace Stateflows.Extensions.MicrosoftAgentFramework.Agents.Classes;

internal abstract class ChatMessageConverterHolder
{
    public abstract Task<ChatMessage[]> ConvertAsync(IAIAgentContext context, CancellationToken cancellationToken = default);
}

internal sealed class ChatMessageConverterHolder<TSource>(IChatMessageConverter<TSource> converter) : ChatMessageConverterHolder
{
    public override Task<ChatMessage[]> ConvertAsync(IAIAgentContext context, CancellationToken cancellationToken = default)
        => Task.WhenAll(
            context
                    .GetTokensOfType<TSource>()
                    .Select(token => converter.ConvertAsync(token, context, cancellationToken))
                    .ToArray()
        );
}

internal sealed class StructuralChatMessageConverterHolder(string eventName, IChatMessageConverter<StructuralEvent> converter) : ChatMessageConverterHolder
{
    public override Task<ChatMessage[]> ConvertAsync(IAIAgentContext context, CancellationToken cancellationToken = default)
        => Task.WhenAll(
            context
                .GetTokensOfType<StructuralEvent>()
                .Where(@event => @event.Name == eventName)
                .Select(@event => converter.ConvertAsync(@event, context, cancellationToken))
                .ToArray()
        );
}
