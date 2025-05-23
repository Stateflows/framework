using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Activities;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Activities.Classes
{
    internal class ActivityWrapper : IActivityBehavior, IInjectionScope
    {
        BehaviorId IBehavior.Id => Behavior.Id;

        public IServiceProvider ServiceProvider => (Behavior as IInjectionScope)?.ServiceProvider;

        private IBehavior Behavior { get; }

        public ActivityWrapper(IBehavior consumer)
        {
            Behavior = consumer;
        }

        public Task<RequestResult<TokensOutput>> SendInputAsync(Action<ITokensInput> tokensAction, IEnumerable<EventHeader> headers = null)
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

        public Task<SendResult> SendAsync<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)
            => Behavior.SendAsync(@event, headers);

        public Task<RequestResult<TResponse>> RequestAsync<TResponse>(IRequest<TResponse> request, IEnumerable<EventHeader> headers = null)
            => Behavior.RequestAsync(request, headers);

        public Task<IWatcher> WatchAsync<TNotification>(Action<TNotification> handler)
            => Behavior.WatchAsync(handler);

        public Task<IWatcher> WatchAsync(string[] notificationNames, Action<EventHolder> handler)
            => Behavior.WatchAsync(notificationNames, handler);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
            => Behavior.Dispose();

        ~ActivityWrapper()
            => Dispose(false);
    }
}
