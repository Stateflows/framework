using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Actions.Classes
{
    internal class ActionWrapper : IActionBehavior, IInjectionScope
    {
        BehaviorId IBehavior.Id => Behavior.Id;

        public IServiceProvider ServiceProvider => (Behavior as IInjectionScope)?.ServiceProvider;
        
        private IBehavior Behavior { get; }

        public ActionWrapper(IBehavior consumer)
        {
            Behavior = consumer;
        }

        public Task<RequestResult<TokensOutput>> SendInputAsync(Action<ITokensInput> tokensAction, IDictionary<string, EventHeader> headers = null)
        {
            var stream = new TokensInput();
            tokensAction(stream);
            return RequestAsync(stream, headers);
        }

        public Task<RequestResult<TokensOutput>> SendInputAsync<TToken>(params TToken[] tokens)
        {
            var stream = new TokensInput<TToken>()
            {
                Tokens = tokens
                    .Select(token => new TokenHolder<TToken>() { Payload = token } as TokenHolder)
                    .ToList()
            };

            return RequestAsync(stream);
        }

        public Task<SendResult> SendAsync<TEvent>(TEvent @event, IDictionary<string, EventHeader> headers = null)
            => Behavior.SendAsync(@event, headers);

        public Task<RequestResult<TResponse>> RequestAsync<TResponse>(IRequest<TResponse> request, IDictionary<string, EventHeader> headers = null)
            => Behavior.RequestAsync(request, headers);

        [Obsolete("Use WatchStatusAsync instead.")]
        public Task<IEnumerable<TNotification>> GetNotificationsAsync<TNotification>(
            DateTime? lastNotificationsCheck = null)
            => Behavior.GetNotificationsAsync<TNotification>(lastNotificationsCheck);
        
        [Obsolete("Use WatchStatusAsync instead.")]
        public Task<IEnumerable<EventHolder>> GetNotificationsAsync(string[] notificationNames,
            DateTime? lastNotificationsCheck = null)
            => Behavior.GetNotificationsAsync(notificationNames, lastNotificationsCheck);

        public Task<IWatcher> WatchAsync<TNotification>(Action<TNotification> handler, DateTime? replayNotificationsSince = null)
            => Behavior.WatchAsync(handler, replayNotificationsSince);

        public Task<IWatcher> WatchAsync(string[] notificationNames, Action<EventHolder> handler, DateTime? replayNotificationsSince = null)
            => Behavior.WatchAsync(notificationNames, handler, replayNotificationsSince);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
            => Behavior.Dispose();

        ~ActionWrapper()
            => Dispose(false);
    }
}
