using Microsoft.Extensions.AI;
using Stateflows.MAF.AIAgents;

namespace Stateflows.Extensions.MicrosoftAgentFramework.Agents.Classes;

internal sealed class DelegateChatMessageConverter<TSource>(Func<IAIAgentContext, TSource, Task<ChatMessage>> converter) : IChatMessageConverter<TSource>
{
    public Task<ChatMessage> ConvertAsync(TSource source, IAIAgentContext context, CancellationToken cancellationToken = default)
        => converter(context, source);
}