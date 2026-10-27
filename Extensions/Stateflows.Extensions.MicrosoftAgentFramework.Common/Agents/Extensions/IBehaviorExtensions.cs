using Microsoft.Extensions.AI;
using Stateflows.Common;

namespace Stateflows;

/// <summary>Provides chat-message operations for Stateflows behaviors integrated with Microsoft Agent Framework.</summary>
public static class IBehaviorExtensions
{
    /// <summary>Watches for incoming chat messages and asynchronously sends each handler response.</summary>
    /// <param name="behavior">The agent behavior to watch.</param>
    /// <param name="chatMessageHandler">Asynchronously handles the incoming message and produces its response.</param>
    /// <returns>A watcher that can be disposed to stop handling messages.</returns>
    public static Task<IWatcher> WatchChatMessagesAsync(this IBehavior behavior, Func<ChatMessage, Task<ChatMessage>> chatMessageHandler)
        => behavior.WatchAsync<ChatMessage>(async functionInvocation =>
            await behavior.SendAsync(await chatMessageHandler(functionInvocation))
        );
        
    /// <summary>Watches for incoming chat messages and synchronously sends each handler response.</summary>
    /// <param name="behavior">The agent behavior to watch.</param>
    /// <param name="chatMessageHandler">Handles the incoming message and produces its response.</param>
    /// <returns>A watcher that can be disposed to stop handling messages.</returns>
    public static Task<IWatcher> WatchChatMessagesAsync(this IBehavior behavior, Func<ChatMessage, ChatMessage> chatMessageHandler)
        => behavior.WatchAsync<ChatMessage>(functionInvocation =>
            behavior.SendAsync(chatMessageHandler(functionInvocation))
        );
}