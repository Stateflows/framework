using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Activities;
using Stateflows.Activities.Events;

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

        public Task<SendResult> SendInputAsync(Action<ITokensInput> tokensAction)
        {
            var stream = new TokensInput();
            tokensAction(stream);
            return SendAsync(stream);
        }

        public Task<SendResult> SendInputAsync<TToken>(params TToken[] tokens)
        {
            var stream = new TokensInput<TToken>()
            {
                Tokens = tokens
                    .Select(token => new TokenHolder<TToken>() { Payload = token } as TokenHolder)
                    .ToList()
            };

            return SendAsync(stream);
        }

        public Task WatchOutputAsync(Action<ITokensOutput> handler)
            => Behavior.WatchAsync<TokensOutput>(handler);

        public Task WatchOutputAsync<TToken>(Action<IEnumerable<TToken>> handler)
            => Behavior.WatchAsync<TokensOutput<TToken>>(output => handler(output.GetAll()));

        public Task UnwatchOutputAsync()
            => Behavior.UnwatchAsync<TokensOutput>();

        public Task UnwatchOutputAsync<TToken>()
            => Behavior.UnwatchAsync<TokensOutput<TToken>>();

        public Task<SendResult> SendAsync<TEvent>(TEvent @event, params EventHeader[] headers)
            => Behavior.SendAsync(@event, headers);

        public Task<RequestResult<TResponse>> RequestAsync<TResponse>(IRequest<TResponse> request, params EventHeader[] headers)
            => Behavior.RequestAsync(request, headers);

        public Task WatchAsync<TNotificationEvent>(Action<TNotificationEvent> handler)
            => Behavior.WatchAsync<TNotificationEvent>(handler);

        public Task UnwatchAsync<TNotificationEvent>()
            => Behavior.UnwatchAsync<TNotificationEvent>();

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
