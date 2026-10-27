using Microsoft.Extensions.AI;
using Stateflows.Common;
using Stateflows.Actions;

namespace Stateflows.MAF.AIAgents
{
    /// <summary>Exposes agent-specific chat message watch operations on a Stateflows action behavior.</summary>
    public interface IAIAgentBehavior : IActionBehavior
    {
        /// <summary>Watches for chat messages, asynchronously processes each one, and sends the formatter's response.</summary>
        /// <param name="chatMessageHandler">Asynchronously handles an incoming chat message and returns the response to send.</param>
        /// <returns>A watcher that can be disposed to stop handling chat messages.</returns>
        public Task<IWatcher> WatchChatMessagesAsync(Func<ChatMessage, Task<ChatMessage>> chatMessageHandler)
            => WatchAsync<ChatMessage>(async functionInvocation =>
                await SendAsync(await chatMessageHandler(functionInvocation))
            );
        
        /// <summary>Watches for chat messages, synchronously processes each one, and sends the formatter's response.</summary>
        /// <param name="chatMessageHandler">Handles an incoming chat message and returns the response to send.</param>
        /// <returns>A watcher that can be disposed to stop handling chat messages.</returns>
        public Task<IWatcher> WatchChatMessagesAsync(Func<ChatMessage, ChatMessage> chatMessageHandler)
            => WatchAsync<ChatMessage>(functionInvocation =>
                SendAsync(chatMessageHandler(functionInvocation))
            );
    }
}
