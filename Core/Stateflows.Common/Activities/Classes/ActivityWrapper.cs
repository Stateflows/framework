using System;
using System.Threading.Tasks;
using System.Collections.Generic;
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

        public Task<RequestResult<ExecutionResponse>> ExecuteAsync(InitializationRequest initializationRequest = null, IEnumerable<Object> inputTokens = null)
        {
            var executionRequest = new ExecutionRequest(initializationRequest ?? new InitializationRequest(), inputTokens ?? new Object[0]);
            if (initializationRequest != null)
            {
                executionRequest.Headers.AddRange(initializationRequest.Headers);
            }

            return Behavior.RequestAsync(executionRequest);
        }

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
