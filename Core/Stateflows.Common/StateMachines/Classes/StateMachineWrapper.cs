using System;
using System.Threading.Tasks;
using Stateflows.StateMachines;

namespace Stateflows.Common.StateMachines.Classes
{
    internal class StateMachineWrapper : IStateMachineBehavior
    {
        public BehaviorId Id => Behavior.Id;

        private IBehavior Behavior { get; }

        public StateMachineWrapper(IBehavior consumer)
        {
            Behavior = consumer;
        }

        public Task<SendResult> SendAsync<TEvent>(TEvent @event, params EventHeader[] headers)
            => Behavior.SendAsync(@event);

        public Task<RequestResult<TResponse>> RequestAsync<TResponse>(IRequest<TResponse> request, params EventHeader[] headers)
            => Behavior.RequestAsync(request);

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

        ~StateMachineWrapper()
            => Dispose(false);
    }
}
