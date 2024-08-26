using System;
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

        public Task<RequestResult<ExecutionResponse>> ExecuteAsync(Event initializationEvent, Action<IInputContainer> inputBuilder = null)
        {
            var request = new ExecutionRequest() { InitializationEvent = initializationEvent };

            inputBuilder?.Invoke(request);

            return Behavior.RequestAsync(request);
        }

        public Task<RequestResult<ExecutionResponse>> ExecuteAsync(Action<IInputContainer> inputBuilder = null)
        {
            var request = new ExecutionRequest();

            inputBuilder?.Invoke(request);

            return Behavior.RequestAsync(request);
        }

        public Task<SendResult> SendAsync<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)
            => Behavior.SendAsync(@event, headers);

        public Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request, IEnumerable<EventHeader> headers = null)
            => Behavior.RequestAsync(request, headers);

        public Task WatchAsync<TNotification>(Action<TNotification> handler)
            => Behavior.WatchAsync<TNotification>(handler);

        public Task UnwatchAsync<TNotification>()
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
