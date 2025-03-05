using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Activities;

namespace Stateflows.Common.Activities.Classes
{
    internal class ActivityWrapper : IActivityBehavior
    {
        BehaviorId IBehavior.Id => Behavior.Id;

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
