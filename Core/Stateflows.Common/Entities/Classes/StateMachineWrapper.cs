using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using Stateflows.Common.Interfaces;
using Stateflows.Entities;

namespace Stateflows.Common.Entities.Classes
{
    internal class EntityWrapper : IEntityBehavior, IInjectionScope
    {
        BehaviorId IBehavior.Id => Behavior.Id;

        public IServiceProvider ServiceProvider => (Behavior as IInjectionScope)?.ServiceProvider;

        private IBehavior Behavior { get; }

        public EntityWrapper(IBehavior consumer)
        {
            Behavior = consumer;
        }

        [DebuggerHidden]
        public Task<SendResult> SendAsync<TEvent>(TEvent @event, IDictionary<string, EventHeader> headers = null)
            => Behavior.SendAsync(@event, headers);

        [DebuggerHidden]
        public Task<RequestResult<TResponse>> RequestAsync<TResponse>(IRequest<TResponse> request, IDictionary<string, EventHeader> headers = null)
            => Behavior.RequestAsync(request, headers);

        public Task<IEnumerable<TNotification>> GetNotificationsAsync<TNotification>(
            DateTime? lastNotificationsCheck = null)
            => Behavior.GetNotificationsAsync<TNotification>(lastNotificationsCheck);

        public Task<IEnumerable<EventHolder>> GetNotificationsAsync(string[] notificationNames,
            DateTime? lastNotificationsCheck = null)
            => Behavior.GetNotificationsAsync(notificationNames, lastNotificationsCheck);

        public Task<IWatcher> WatchAsync<TNotification>(Action<TNotification> handler, DateTime? replayNotificatonsSince = null)
            => Behavior.WatchAsync(handler, replayNotificatonsSince);

        public Task<IWatcher> WatchAsync(string[] notificationNames, Action<EventHolder> handler, DateTime? replayNotificatonsSince = null)
            => Behavior.WatchAsync(notificationNames, handler, replayNotificatonsSince);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
            => Behavior.Dispose();

        ~EntityWrapper()
            => Dispose(false);
    }
}
