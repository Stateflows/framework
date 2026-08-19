using Microsoft.Extensions.AI;
using Stateflows.Common;
using Stateflows.Actions;

namespace Stateflows.MAF.AIAgents
{
    public interface IAIAgentBehavior : IActionBehavior
    {
        public Task<IWatcher> WatchChatMessagesAsync(Func<ChatMessage, Task<ChatMessage>> chatMessageHandler)
            => WatchAsync<ChatMessage>(async functionInvocation =>
                await SendAsync(await chatMessageHandler(functionInvocation))
            );
        
        public Task<IWatcher> WatchChatMessagesAsync(Func<ChatMessage, ChatMessage> chatMessageHandler)
            => WatchAsync<ChatMessage>(functionInvocation =>
                SendAsync(chatMessageHandler(functionInvocation))
            );
    }
}
