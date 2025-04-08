using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Subscription;

namespace Stateflows.Common.Classes
{
    internal sealed class StandaloneBehavior : IBehavior, IUnwatcher, INotificationHandler
    {
        public BehaviorId Id { get; }

        private readonly StateflowsEngine engine;
        private readonly IServiceProvider serviceProvider;
        private readonly NotificationsHub notificationsHub;
        private readonly Dictionary<string, List<Action<EventHolder>>> handlers = new Dictionary<string, List<Action<EventHolder>>>();

        public StandaloneBehavior(StateflowsEngine engine, IServiceProvider serviceProvider, BehaviorId id)
        {
            this.engine = engine;
            this.serviceProvider = serviceProvider;
            notificationsHub = new NotificationsHub(serviceProvider.GetRequiredService<IStateflowsCache>());
            notificationsHub.RegisterHandler(this);
            
            Id = id;
        }

        public Task HandleNotificationAsync(EventHolder eventHolder)
        {
            if (eventHolder.SenderId == Id)
            {
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

            return Task.CompletedTask;
        }

        [DebuggerHidden]
        public async Task<SendResult> SendAsync<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)
        {
            var eventHolder = @event.ToTypedEventHolder(headers);
            var executionToken = new ExecutionToken(Id, eventHolder, serviceProvider);
            await engine.HandleEventAsync(executionToken);

            if (ResponseHolder.ResponsesAreSet())
            {
                ResponseHolder.CopyResponses(executionToken.Responses);
            }

            return new SendResult(
                executionToken.EventHolder, 
                executionToken.Status, 
                null, // todo: get notifications
                executionToken.Validation
            );
        }

        [DebuggerHidden]
        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(IRequest<TResponse> request, IEnumerable<EventHeader> headers = null)
        {
            var eventHolder = request.ToTypedEventHolder(headers);
            var executionToken = new ExecutionToken(Id, eventHolder, serviceProvider);
            await engine.HandleEventAsync(executionToken);

            if (ResponseHolder.ResponsesAreSet())
            {
                ResponseHolder.CopyResponses(executionToken.Responses);
            }
            else
            {
                ResponseHolder.SetResponses(executionToken.Responses);
            }

            var result = new RequestResult<TResponse>(
                executionToken.EventHolder, 
                executionToken.Status,
                null, // todo: get notifications 
                executionToken.Validation
            );

            return result;
        }
        
        public async Task<IWatcher> WatchAsync<TNotification>(Action<TNotification> handler)
        {
            lock (handlers)
            {
                var notificationName = typeof(TNotification).GetEventName();
                if (!handlers.TryGetValue(notificationName, out var notificationHandlers))
                {
                    notificationHandlers = new List<Action<EventHolder>>();
                    handlers.Add(notificationName, notificationHandlers);
                }

                notificationHandlers.Add(eventHolder => handler(((EventHolder<TNotification>)eventHolder).Payload));
            }

            var lastNotificationCheck = DateTime.Now;
            var pendingNotifications = await notificationsHub.GetNotificationsAsync(
                Id,
                notification =>
                    notification is EventHolder<TNotification> &&
                    notification.SentAt.AddSeconds(notification.TimeToLive) >= lastNotificationCheck
            );
            foreach (var pendingNotification in pendingNotifications)
            {
                handler(((EventHolder<TNotification>)pendingNotification).Payload);
            }

            return new Watcher<TNotification>(this);
        }

        public Task UnwatchAsync<TNotification>()
        {
            lock (handlers)
            {
                var notificationName = Event<TNotification>.Name;
                handlers.Remove(notificationName);
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            
            notificationsHub.UnregisterHandler(this);
        }

        ~StandaloneBehavior()
            => Dispose(false);
    }
}
