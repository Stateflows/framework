using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Interfaces;
using Stateflows.Activities;
using Stateflows.Activities.Events;

namespace Stateflows.Common.Activities.Classes
{
    internal class ActivityWrapper : IActivity
    {
        BehaviorId IBehavior.Id => Behavior.Id;

        private IBehavior Behavior { get; }

        public ActivityWrapper(IBehavior consumer)
        {
            Behavior = consumer;
        }

        public async Task<IEnumerable<Token>> ExecuteAsync(InitializationRequest initializationRequest = null)
        {
            if (initializationRequest == null)
            {
                initializationRequest = new InitializationRequest();
            }
            var executionRequest = new ExecutionRequest(initializationRequest);
            executionRequest.Headers.AddRange(initializationRequest.Headers);
            return (await Behavior.RequestAsync(executionRequest)).Response?.OutputTokens ?? new Token[0];

        }

        public async Task<T> ExecuteAsync<T>(InitializationRequest initializationRequest = null)
        {
            var results = (await ExecuteAsync(initializationRequest)).OfType<ValueToken<T>>();

            return results.Any()
                ? results.First().Value
                : default;
        }

        public Task Cancel()
            => SendAsync(new CancelRequest());

        public Task<SendResult> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
            => Behavior.SendAsync(@event);

        public Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
            => Behavior.RequestAsync(request);
    }
}
