using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Utils;
using Stateflows.Common.Utilities;
using Stateflows.Common.Subscription;

namespace Stateflows.Common.Classes
{
    internal class Behavior : IBehavior, IUnwatcher
    {
        public BehaviorId Id { get; }

        private readonly StateflowsEngine engine;
        private readonly IServiceProvider serviceProvider;
        private readonly NotificationsHub subscriptionHub;
        private readonly Dictionary<string, List<Action<EventHolder>>> handlers = new Dictionary<string, List<Action<EventHolder>>>();

        public Behavior(StateflowsEngine engine, IServiceProvider serviceProvider, BehaviorId id)
        {
            this.engine = engine;
            this.serviceProvider = serviceProvider;
            subscriptionHub = serviceProvider.GetRequiredService<NotificationsHub>();
            subscriptionHub.OnPublish += SubscriptionHub_OnPublish;
            Id = id;
        }

        private void SubscriptionHub_OnPublish(EventHolder eventHolder)
        {
            if (eventHolder.SenderId != Id)
            {
                return;
            }

            lock (handlers)
            {
                if (handlers.TryGetValue(eventHolder.Name, out var notificationHandlers))
                {
                    foreach (var handler in notificationHandlers)
                    {
                        handler.Invoke(eventHolder);
                    }
                }
            }
        }

        [DebuggerHidden]
        public async Task<SendResult> SendAsync<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)
        {
            var executionToken = engine.EnqueueEvent(Id, @event.ToEventHolder(@event.GetType()), serviceProvider);
            await executionToken.Handled.WaitOneAsync();

            return new SendResult(executionToken.EventHolder, executionToken.Status, executionToken.Validation);
        }

        [DebuggerHidden]
        public async Task<RequestResult<TResponseEvent>> RequestAsync<TResponseEvent>(IRequest<TResponseEvent> request, IEnumerable<EventHeader> headers = null)
        {
            var executionToken = engine.EnqueueEvent(Id, request.ToEventHolder(request.GetType()), serviceProvider);
            await executionToken.Handled.WaitOneAsync();

            ResponseHolder.SetResponses(executionToken.Responses);

            var result = new RequestResult<TResponseEvent>(executionToken.EventHolder, executionToken.Status, executionToken.Validation);

            ResponseHolder.ClearResponses();

            return result;
        }

        public Task<IWatcher> WatchAsync<TNotificationEvent>(Action<TNotificationEvent> handler)
        {
            lock (handlers)
            {
                var notificationName = typeof(TNotificationEvent).GetEventName();
                if (!handlers.TryGetValue(notificationName, out var notificationHandlers))
                {
                    notificationHandlers = new List<Action<EventHolder>>();
                    handlers.Add(notificationName, notificationHandlers);
                }

                notificationHandlers.Add(eventHolder => handler((eventHolder as EventHolder<TNotificationEvent>).Payload));
            }

            return Task.FromResult((IWatcher)new Watcher<TNotificationEvent>(this));
        }

        public Task UnwatchAsync<TNotificationEvent>()
        {
            lock (handlers)
            {
                var notificationName = Event<TNotificationEvent>.Name;
                handlers.Remove(notificationName);
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
            => subscriptionHub.OnPublish -= SubscriptionHub_OnPublish;

        ~Behavior()
            => Dispose(false);
    }
}
