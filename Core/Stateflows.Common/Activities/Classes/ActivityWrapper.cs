using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Activities;
using Stateflows.Activities.Events;
using System.Linq;

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

        public async Task<RequestResult<ExecutionResponse>> ExecuteAsync(Event initializationEvent, IEnumerable<object> inputTokens = null)
        {
            var executionRequest = new ExecutionRequest() { InputTokens = inputTokens ?? new object[0] };

            var result = await Behavior.SendCompoundAsync(
                initializationEvent,
                executionRequest
            );

            var executionResult = result.Response.Results.Last();

            return new RequestResult<ExecutionResponse>(executionRequest, executionResult.Status, executionResult.Validation);
        }

        public Task<RequestResult<ExecutionResponse>> ExecuteAsync(IEnumerable<object> inputTokens = null)
            => Behavior.RequestAsync(new ExecutionRequest() { InputTokens = inputTokens ?? new object[0] });

        public Task<SendResult> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
            => Behavior.SendAsync(@event);

        public Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
            => Behavior.RequestAsync(request);

        public Task WatchAsync<TNotification>(Action<TNotification> handler)
            where TNotification : Notification, new()
            => Behavior.WatchAsync<TNotification>(handler);

        public Task UnwatchAsync<TNotification>()
            where TNotification : Notification, new()
            => Behavior.UnwatchAsync<TNotification>();

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
