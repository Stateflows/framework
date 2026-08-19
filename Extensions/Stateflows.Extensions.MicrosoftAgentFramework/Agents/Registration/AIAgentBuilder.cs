using Microsoft.Extensions.AI;
using Stateflows.Actions.Registration.Interfaces;
using Stateflows.Extensions.MicrosoftAgentFramework.Agents.Classes;
using Stateflows.MAF.AIAgents.Classes;

namespace Stateflows.MAF.AIAgents.Registration;

internal class AIAgentBuilder(IActionBuilder<AIAgentAction> actionBuilder) : IAIAgentBuilder
{
    public MAF.AIAgents.IAIAgentBuilder SetInitialPrompt(string initialPrompt)
    {
        actionBuilder.Configure(f => f.InitialPrompt = initialPrompt);
        return this;
    }

    public MAF.AIAgents.IAIAgentBuilder AddConsumedEvent<TEvent>(Func<IAIAgentContext, TEvent, Task<ChatMessage>> eventFormatterAsync)
    {
        actionBuilder.Configure(f => f.EventFormatters[typeof(TEvent)] = new ChatMessageConverterHolder<TEvent>(new DelegateChatMessageConverter<TEvent>(eventFormatterAsync)));
        actionBuilder.AddConsumedEvent<TEvent>();
        return this;
    }

    public MAF.AIAgents.IAIAgentBuilder AddConsumedToken<TToken>(Func<IAIAgentContext, TToken, Task<ChatMessage>> tokenFormatterAsync)
    {
        actionBuilder.Configure(f => f.TokenFormatters[typeof(TToken)] = new ChatMessageConverterHolder<TToken>(new DelegateChatMessageConverter<TToken>(tokenFormatterAsync)));
        actionBuilder.AddConsumedToken<TToken>();
        return this;
    }
}