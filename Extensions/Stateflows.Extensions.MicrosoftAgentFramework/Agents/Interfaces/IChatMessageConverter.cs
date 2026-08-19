using Microsoft.Extensions.AI;

namespace Stateflows.MAF.AIAgents;

public interface IChatMessageConverter<in TSource>
{
    Task<ChatMessage> ConvertAsync(TSource source, IAIAgentContext context, CancellationToken cancellationToken = default);
}