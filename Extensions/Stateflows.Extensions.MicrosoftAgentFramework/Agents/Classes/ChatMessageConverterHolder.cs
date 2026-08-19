using Microsoft.Extensions.AI;
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