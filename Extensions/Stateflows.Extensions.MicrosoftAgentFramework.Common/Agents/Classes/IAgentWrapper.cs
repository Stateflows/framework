using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.MAF.AIAgents.Classes
{
    internal class AgentWrapper(IBehavior consumer) : IAIAgentBehavior, IInjectionScope
    {
        BehaviorId IBehavior.Id => Behavior.Id;

        public IServiceProvider ServiceProvider => ((IInjectionScope)Behavior).ServiceProvider;
        
        private IBehavior Behavior { get; } = consumer;

        public Task<RequestResult<TokensOutput>> SendInputAsync(Action<ITokensInput> tokensAction, IDictionary<string, EventHeader>? headers = null)
        {
            // todo
            // (this as IAIAgentBehavior).WatchChatMessagesAsync(i => i.Allow());
            
            var stream = new TokensInput();
            tokensAction(stream);
            return RequestAsync(stream, headers);
        }

        public Task<RequestResult<TokensOutput>> SendInputAsync<TToken>(params TToken[] tokens)
        {
            var stream = new TokensInput<TToken>()
            {
                Tokens = tokens
                    .Select(TokenHolder (token) => new TokenHolder<TToken>() { Payload = token })
                    .ToList()
            };

            return RequestAsync(stream);
        }
        public Task<SendResult> SendAsync<TEvent>(TEvent @event, IDictionary<string, EventHeader>? headers = null)
            => Behavior.SendAsync(@event, headers);

        public Task<RequestResult<TResponse>> RequestAsync<TResponse>(IRequest<TResponse> request, IDictionary<string, EventHeader>? headers = null)
            => Behavior.RequestAsync(request, headers);

        public Task<IEnumerable<TNotification>> GetNotificationsAsync<TNotification>(
            DateTime? lastNotificationsCheck = null)
            => Behavior.GetNotificationsAsync<TNotification>(lastNotificationsCheck);

        public Task<IEnumerable<EventHolder>> GetNotificationsAsync(string[] notificationNames,
            DateTime? lastNotificationsCheck = null)
            => Behavior.GetNotificationsAsync(notificationNames, lastNotificationsCheck);

        public Task<IWatcher> WatchAsync<TNotification>(Action<TNotification> handler, DateTime? replayNotificatonsSince = null)
            => Behavior.WatchAsync(handler, replayNotificatonsSince);

        public Task<IWatcher> WatchAsync(string[] notificationNames, Action<EventHolder> handler, DateTime? replayNotificatonsSince = null)
            => Behavior.WatchAsync(notificationNames, handler, replayNotificatonsSince);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
            => Behavior.Dispose();

        ~AgentWrapper()
            => Dispose(false);
    }
}
