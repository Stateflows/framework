using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Utilities;
using Stateflows.Common.Subscription;

namespace Stateflows.Common.Classes
{
    internal class Behavior : IBehavior
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
            var executionToken = engine.EnqueueEvent(Id, @event, serviceProvider);
            await executionToken.Handled.WaitOneAsync();

            return new SendResult(executionToken.EventHolder, executionToken.Status, executionToken.Validation);
        }

        [DebuggerHidden]
        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request, IEnumerable<EventHeader> headers = null)
        {
            var executionToken = engine.EnqueueEvent(Id, request, serviceProvider);
            await executionToken.Handled.WaitOneAsync();

            return new RequestResult<TResponse>(executionToken.EventHolder as EventHolder<Request<TResponse>>, executionToken.Status, executionToken.Validation);
        }

        public Task WatchAsync<TNotification>(Action<TNotification> handler)
        {
            lock (handlers)
            {
                var notificationName = EventInfo<TNotification>.Name;
                if (!handlers.TryGetValue(notificationName, out var notificationHandlers))
                {
                    notificationHandlers = new List<Action<EventHolder>>();
                    handlers.Add(notificationName, notificationHandlers);
                }

                notificationHandlers.Add(eventHolder => handler((eventHolder as EventHolder<TNotification>).Payload));
            }

            return Task.CompletedTask;
        }

        public Task UnwatchAsync<TNotification>()
        {
            lock (handlers)
            {
                var notificationName = EventInfo<TNotification>.Name;
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
