using System;
using System.Threading.Tasks;
using System.Collections.Generic;
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

        public Task<SendResult> SendAsync<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)
            => Behavior.SendAsync(@event);

        //public Task<SendResult> SendAsync(object @event, Type eventType, IEnumerable<EventHeader> headers = null)
        //    => Behavior.SendAsync(@event, eventType, headers);

        public Task<RequestResult<TResponse>> RequestAsync<TResponse>(IRequest<TResponse> request, IEnumerable<EventHeader> headers = null)
            => Behavior.RequestAsync(request);

        public Task<IWatcher> WatchAsync<TNotificationEvent>(Action<TNotificationEvent> handler)
            => Behavior.WatchAsync(handler);

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
