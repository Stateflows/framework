using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stateflows.Common.Classes;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Subscription
{
    internal class NotificationsHub : IHostedService, INotificationsHub
    {
        public NotificationsHub(IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            var scopedProvider = scope.ServiceProvider;
            Storage = scopedProvider.GetService<IStateflowsNotificationsStorage>();
            CommonInterceptor = scopedProvider.GetService<CommonInterceptor>();
        }

        private readonly IStateflowsNotificationsStorage Storage;

        private readonly CommonInterceptor CommonInterceptor;
        
        private readonly CancellationTokenSource CancellationTokenSource = new();

        private static readonly List<INotificationHandler> Handlers = [];
        
        public Task<IEnumerable<EventHolder>> GetNotificationsAsync(BehaviorId behaviorId, string[] notificationNames,
            DateTime? lastNotificationCheck = null)
            => Storage.GetNotificationsAsync(behaviorId, notificationNames, lastNotificationCheck ?? DateTime.Now);

        public async Task<IEnumerable<TNotification>> GetNotificationsAsync<TNotification>(BehaviorId behaviorId, DateTime? lastNotificationCheck = null)
            => (await Storage.GetNotificationsAsync(behaviorId, new string[] { Event<TNotification>.Name }, lastNotificationCheck ?? DateTime.Now))
                .Select(n => ((EventHolder<TNotification>)n).Payload);

        public void RegisterHandler(INotificationHandler notificationHandler)
        {
            lock (Handlers)
            {
                Handlers.Add(notificationHandler);
            }
        }

        public void UnregisterHandler(INotificationHandler notificationHandler)
        {
            lock (Handlers)
            {
                Handlers.Remove(notificationHandler);
            }
        }

        private static Task RunHandlersAsync(EventHolder eventHolder)
        {
            INotificationHandler[] handlers;
            lock (Handlers)
            {
                handlers = Handlers.ToArray();
            }

            return Task.WhenAll(handlers.Select(handler => handler.HandleNotificationAsync(eventHolder)));
        }   

        public Task PublishAsync(EventHolder eventHolder)
            => PublishRangeAsync([eventHolder]);

        public async Task PublishRangeAsync(IEnumerable<EventHolder> eventHolders)
        {
            var eventHoldersArray = eventHolders as EventHolder[] ?? eventHolders.ToArray();
            
            _ = Task.WhenAll(eventHoldersArray.Select(RunHandlersAsync));
            
            var holdersBySenderIds = eventHoldersArray
                .Where(h => h.SenderId != null)
                .GroupBy(h => (BehaviorId)h.SenderId);
            
            foreach (var group in holdersBySenderIds)
            {
                await Storage.SaveNotificationsAsync(group.Key, group.ToArray());
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            CancellationTokenSource.Cancel();

            return Task.CompletedTask;
        }
    }
}
