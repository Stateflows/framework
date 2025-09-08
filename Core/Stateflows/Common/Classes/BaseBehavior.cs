using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows.Common.Classes
{
    internal abstract class BaseBehavior : IBehavior, IUnwatcher, INotificationHandler, IInjectionScope
    {
        public BehaviorId Id { get; }

        protected readonly IServiceProvider serviceProvider;
        private readonly INotificationsHub notificationsHub;
        private readonly IStateflowsTenantProvider tenantProvider;
        private readonly ITenantAccessor tenantAccessor;
        private readonly Dictionary<string, List<Action<EventHolder>>> handlers = new Dictionary<string, List<Action<EventHolder>>>();
        private readonly Dictionary<IWatcher, Action<EventHolder>> handlersByWatcher = new Dictionary<IWatcher, Action<EventHolder>>();

        public BaseBehavior(IServiceProvider serviceProvider, BehaviorId id, INotificationsHub notificationsHub)
        {
            this.serviceProvider = serviceProvider;
            this.notificationsHub = notificationsHub;
            this.notificationsHub.RegisterHandler(this);

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
                            handler.Invoke(eventHolder);
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        protected abstract Task<ExecutionToken> ProcessEventAsync(BehaviorId id, EventHolder eventHolder);

        [DebuggerHidden]
        public async Task<SendResult> SendAsync<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)
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
        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(IRequest<TResponse> request, IEnumerable<EventHeader> headers = null)
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
        
        public async Task<IWatcher> WatchAsync<TNotification>(Action<TNotification> handler)
        {
            var watcher = new Watcher(this);
            lock (handlers)
            {
                var notificationName = typeof(TNotification).GetEventName();
                if (!handlers.TryGetValue(notificationName, out var notificationHandlers))
                {
                    notificationHandlers = new List<Action<EventHolder>>();
                    handlers.Add(notificationName, notificationHandlers);
                }

                Action<EventHolder> notificationHandler = eventHolder => handler(((EventHolder<TNotification>)eventHolder).Payload);
                notificationHandlers.Add(notificationHandler);
                handlersByWatcher[watcher] = notificationHandler;
            }

            tenantAccessor.CurrentTenantId = await tenantProvider.GetCurrentTenantIdAsync();
            var lastNotificationCheck = DateTime.Now;
            var pendingNotifications = await notificationsHub.GetNotificationsAsync<TNotification>(
                Id,
                lastNotificationCheck
            );
            
            foreach (var pendingNotification in pendingNotifications)
            {
                handler(pendingNotification);
            }

            return watcher;
        }

        public async Task<IWatcher> WatchAsync(string[] notificationNames, Action<EventHolder> handler)
        {
            var watcher = new Watcher(this);
            lock (handlers)
            {
                foreach (var notificationName in notificationNames)
                {
                    if (!handlers.TryGetValue(notificationName, out var notificationHandlers))
                    {
                        notificationHandlers = new List<Action<EventHolder>>();
                        handlers.Add(notificationName, notificationHandlers);
                    }

                    notificationHandlers.Add(handler);
                }
                
                handlersByWatcher[watcher] = handler;
            }

            tenantAccessor.CurrentTenantId = await tenantProvider.GetCurrentTenantIdAsync();
            var lastNotificationCheck = DateTime.Now;
            var pendingNotifications = await notificationsHub.GetNotificationsAsync(
                Id,
                notificationNames,
                lastNotificationCheck
            );
            
            foreach (var pendingNotification in pendingNotifications)
            {
                handler(pendingNotification);
            }

            return watcher;
        }

        public Task UnwatchAsync(IWatcher watcher)
        {
            lock (handlers)
            {
                if (handlersByWatcher.TryGetValue(watcher, out var handler))
                {
                    foreach (var handlersList in handlers.Values)
                    {
                        handlersList.Remove(handler);
                    }

                    foreach (var emptyItem in handlers.Where(h => !h.Value.Any()))
                    {
                        handlers.Remove(emptyItem.Key);
                    }

                    handlersByWatcher.Remove(watcher);
                }
            }

            return Task.CompletedTask;
        }
        
        public Task<IEnumerable<TNotification>> GetNotificationsAsync<TNotification>(DateTime? lastNotificationsCheck = null)
            => notificationsHub.GetNotificationsAsync<TNotification>(
                Id,
                lastNotificationsCheck
            );
        
        public Task<IEnumerable<EventHolder>> GetNotificationsAsync(string[] notificationNames, DateTime? lastNotificationsCheck = null)
            => notificationsHub.GetNotificationsAsync(
                Id,
                notificationNames,
                lastNotificationsCheck
            );

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
