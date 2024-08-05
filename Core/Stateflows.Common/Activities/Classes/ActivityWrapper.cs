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

        public Task<RequestResult<ExecutionResponse>> ExecuteAsync(Event initializationEvent, IEnumerable<object> inputTokens = null)
            => Behavior.RequestAsync(new ExecutionRequest()
            {
                InitializationEvent = initializationEvent,
                InputTokens = inputTokens ?? new object[0],
            });

        public Task<RequestResult<ExecutionResponse>> ExecuteAsync(IEnumerable<object> inputTokens = null)
            => Behavior.RequestAsync(new ExecutionRequest() { InputTokens = inputTokens ?? new object[0] });

        public Task<SendResult<TEvent>> SendAsync<TEvent>(TEvent @event)
            => Behavior.SendAsync(@event);

        public Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
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
