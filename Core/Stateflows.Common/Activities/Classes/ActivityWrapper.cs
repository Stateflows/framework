using System;
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
