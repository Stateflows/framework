using Microsoft.Extensions.AI;
using Stateflows.Common;

namespace Stateflows;

public static class IBehaviorExtensions
{
    public static Task<IWatcher> WatchChatMessagesAsync(this IBehavior behavior, Func<ChatMessage, Task<ChatMessage>> chatMessageHandler)
        => behavior.WatchAsync<ChatMessage>(async functionInvocation =>
            await behavior.SendAsync(await chatMessageHandler(functionInvocation))
        );
        
    public static Task<IWatcher> WatchChatMessagesAsync(this IBehavior behavior, Func<ChatMessage, ChatMessage> chatMessageHandler)
        => behavior.WatchAsync<ChatMessage>(functionInvocation =>
            behavior.SendAsync(chatMessageHandler(functionInvocation))
        );
}