using Microsoft.Extensions.AI;

namespace Stateflows.MAF.AIAgents;

public interface IAIAgentBuilder
{
    IAIAgentBuilder SetName(string name);
    IAIAgentBuilder SetDescription(string description);
    IAIAgentBuilder SetInitialPrompt(string initialPrompt);
    IAIAgentBuilder AddConsumedEvent<TEvent>(Func<IAIAgentContext, TEvent, Task<ChatMessage>> eventFormatterAsync);
    IAIAgentBuilder AddConsumedToken<TToken>(Func<IAIAgentContext, TToken, Task<ChatMessage>> tokenFormatterAsync);
}