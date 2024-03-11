using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Utilities;
using Stateflows.Common.Subscription;

namespace Stateflows.Common.Classes
{
    internal class Behavior : IBehavior, IDisposable
    {
        public BehaviorId Id { get; }

        private readonly StateflowsEngine engine;
        private readonly IServiceProvider serviceProvider;
        private readonly NotificationsHub subscriptionHub;
        private readonly Dictionary<string, List<Action<Notification>>> handlers = new Dictionary<string, List<Action<Notification>>>();

        public Behavior(StateflowsEngine engine, IServiceProvider serviceProvider, BehaviorId id)
        {
            this.engine = engine;
            this.serviceProvider = serviceProvider;
            subscriptionHub = serviceProvider.GetRequiredService<NotificationsHub>();
            subscriptionHub.OnPublish += SubscriptionHub_OnPublish;
            Id = id;
        }

        private void SubscriptionHub_OnPublish(Notification notification)
        {
            lock (handlers)
            {
                if (handlers.TryGetValue(notification.Name, out var notificationHandlers))
                {
                    foreach (var handler in notificationHandlers)
                    {
                        handler.Invoke(notification);
                    }
                }
            }
        }

        public async Task<SendResult> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
        {
            var holder = engine.EnqueueEvent(Id, @event, serviceProvider);
            await holder.Handled.WaitOneAsync();

            return new SendResult(@event, holder.Status, holder.Validation);
        }

        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            var holder = engine.EnqueueEvent(Id, request, serviceProvider);
            await holder.Handled.WaitOneAsync();

            return new RequestResult<TResponse>(request, holder.Status, holder.Validation);
        }

        public Task WatchAsync<TNotification>(Action<TNotification> handler)
            where TNotification : Notification, new()
        {
            lock (handlers)
            {
                var notificationName = EventInfo<TNotification>.Name;
                if (!handlers.TryGetValue(notificationName, out var notificationHandlers))
                {
                    notificationHandlers = new List<Action<Notification>>();
                    handlers.Add(notificationName, notificationHandlers);
                }

                notificationHandlers.Add(n => handler(n as TNotification));
            }

            return Task.CompletedTask;
        }

        public Task UnwatchAsync<TNotification>()
            where TNotification : Notification, new()
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
            subscriptionHub.OnPublish -= SubscriptionHub_OnPublish;
        }
    }
}
