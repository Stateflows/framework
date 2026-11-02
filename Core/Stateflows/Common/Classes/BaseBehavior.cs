using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows.Common.Classes
{
    internal abstract class BaseBehavior : IBehavior, IUnwatcher, INotificationHandler, IInjectionScope
    {
        public BehaviorId Id { get; }

        private readonly IServiceProvider serviceProvider;
        private readonly INotificationsHub notificationsHub;
        private readonly IStateflowsTenantProvider tenantProvider;
        private readonly ITenantAccessor tenantAccessor;
        private sealed class Registration(Func<EventHolder, Task> handler)
        {
            public Func<EventHolder, Task> Handler { get; } = handler;
            public Task Tail { get; set; } = Task.CompletedTask;
            public bool Active { get; set; } = true;
        }
        private readonly Dictionary<string, List<Registration>> handlers = new();
        private readonly Dictionary<IWatcher, Registration> handlersByWatcher = new();
        private readonly ILogger<BaseBehavior> logger;

        public BaseBehavior(IServiceProvider serviceProvider, BehaviorId id, INotificationsHub notificationsHub)
        {
            this.serviceProvider = serviceProvider;
            this.notificationsHub = notificationsHub;
            this.notificationsHub.RegisterHandler(this);
            logger = serviceProvider.GetRequiredService<ILogger<BaseBehavior>>();

            tenantAccessor = serviceProvider.GetRequiredService<ITenantAccessor>();
            tenantProvider = serviceProvider.GetRequiredService<IStateflowsTenantProvider>();
            
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
                            EnqueueNotification(handler, eventHolder);
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        private void EnqueueNotification(Registration registration, EventHolder notification)
            => registration.Tail = InvokeHandlerAsync(registration.Tail, registration, notification);

        private async Task InvokeHandlerAsync(Task previous, Registration registration, EventHolder notification)
        {
            await previous;
            try
            {
                lock (handlers)
                {
                    if (!registration.Active)
                        return;
                }
                await registration.Handler(notification);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Notification handler failed for {BehaviorId}, notification {NotificationName}.",
                    Id, notification.Name);
            }
        }

        private Task WaitForDeliveryAsync(Registration registration)
        {
            lock (handlers)
                return registration.Tail;
        }

        protected abstract Task<ExecutionToken> ProcessEventAsync(BehaviorId id, EventHolder eventHolder);

        [DebuggerHidden]
        public async Task<SendResult> SendAsync<TEvent>(TEvent @event, IDictionary<string, EventHeader> headers = null)
        {
            var eventHolder = @event.ToTypedEventHolder(headers);
            var executionToken = await ProcessEventAsync(Id, eventHolder);

            if (ResponseHolder.ResponsesAreSet())
            {
                ResponseHolder.CopyResponses(executionToken.Responses);
            }

            return new SendResult( 
                executionToken.Status, 
                executionToken.Validation
            );
        }

        [DebuggerHidden]
        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(IRequest<TResponse> request, IDictionary<string, EventHeader> headers = null)
        {
            var eventHolder = request.ToTypedEventHolder(headers);
            var executionToken = await ProcessEventAsync(Id, eventHolder);

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
                executionToken.Validation
            );

            return result;
        }
        
        public async Task<IWatcher> WatchAsync<TNotification>(Func<TNotification, Task> asyncHandler, DateTime? replayNotificationsSince = null)
        {
            var registration = new Registration(eventHolder => asyncHandler(((EventHolder<TNotification>)eventHolder).Payload));
            var watcher = new Watcher(this, () => WaitForDeliveryAsync(registration));
            lock (handlers)
            {
                var notificationName = typeof(TNotification).GetEventName();
                if (!handlers.TryGetValue(notificationName, out var notificationHandlers))
                {
                    notificationHandlers = [];
                    handlers.Add(notificationName, notificationHandlers);
                }

                notificationHandlers.Add(registration);
                handlersByWatcher[watcher] = registration;
            }

            tenantAccessor.CurrentTenantId = tenantProvider.GetCurrentTenantId();
            var lastNotificationCheck = replayNotificationsSince ?? DateTime.Now;
            var pendingNotifications = await notificationsHub.GetNotificationsAsync<TNotification>(
                Id,
                lastNotificationCheck
            );
            
            foreach (var pendingNotification in pendingNotifications)
            {
                lock (handlers)
                    EnqueueNotification(registration,
                        new EventHolder<TNotification> { Payload = pendingNotification, SenderId = Id });
            }

            return watcher;
        }

        public async Task<IWatcher> WatchAsync(string[] notificationNames, Func<EventHolder, Task> asyncHandler, DateTime? replayNotificationsSince = null)
        {
            var registration = new Registration(asyncHandler);
            var watcher = new Watcher(this, () => WaitForDeliveryAsync(registration));
            lock (handlers)
            {
                foreach (var notificationName in notificationNames)
                {
                    if (!handlers.TryGetValue(notificationName, out var notificationHandlers))
                    {
                        notificationHandlers = [];
                        handlers.Add(notificationName, notificationHandlers);
                    }

                    notificationHandlers.Add(registration);
                }
                
                handlersByWatcher[watcher] = registration;
            }

            tenantAccessor.CurrentTenantId = tenantProvider.GetCurrentTenantId();
            var lastNotificationCheck = replayNotificationsSince ?? DateTime.Now;
            var pendingNotifications = await notificationsHub.GetNotificationsAsync(
                Id,
                notificationNames,
                lastNotificationCheck
            );
            
            foreach (var pendingNotification in pendingNotifications)
            {
                lock (handlers)
                    EnqueueNotification(registration, pendingNotification);
            }

            return watcher;
        }

        public Task UnwatchAsync(IWatcher watcher)
        {
            lock (handlers)
            {
                if (handlersByWatcher.TryGetValue(watcher, out var handler))
                {
                    handler.Active = false;
                    foreach (var handlersList in handlers.Values)
                    {
                        handlersList.Remove(handler);
                    }

                    foreach (var emptyItem in handlers.Where(h => !h.Value.Any()).ToArray())
                    {
                        handlers.Remove(emptyItem.Key);
                    }

                    handlersByWatcher.Remove(watcher);
                }
            }

            return Task.CompletedTask;
        }

        [Obsolete("Use WatchStatusAsync instead.")]
        public async Task<IEnumerable<TNotification>> GetNotificationsAsync<TNotification>(
            DateTime? lastNotificationsCheck = null)
        {
            tenantAccessor.CurrentTenantId = tenantProvider.GetCurrentTenantId();
            return await notificationsHub.GetNotificationsAsync<TNotification>(
                Id,
                lastNotificationsCheck
            );
        }

        [Obsolete("Use WatchStatusAsync instead.")]
        public async Task<IEnumerable<EventHolder>> GetNotificationsAsync(string[] notificationNames,
            DateTime? lastNotificationsCheck = null)
        {
            tenantAccessor.CurrentTenantId = tenantProvider.GetCurrentTenantId();
            return await notificationsHub.GetNotificationsAsync(
                Id,
                notificationNames,
                lastNotificationsCheck
            );
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            notificationsHub?.UnregisterHandler(this);
        }

        ~BaseBehavior()
            => Dispose(false);

        public IServiceProvider ServiceProvider => serviceProvider;
    }
}
