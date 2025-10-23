using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows.Common.Subscription
{
    internal class NotificationsHub : IHostedService, INotificationsHub
    {
        // public NotificationsHub(IStateflowsCache cache)
        // {
        //     Cache = cache;
        // }

        public NotificationsHub(IServiceProvider serviceProvider)
        {
            Storage = serviceProvider.CreateScope().ServiceProvider.GetService<IStateflowsNotificationsStorage>();
        }

        // private readonly IStateflowsCache Cache;

        private readonly IStateflowsNotificationsStorage Storage;
        
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        private static readonly List<INotificationHandler> Handlers = new List<INotificationHandler>();

        // private Task UpdateNotificationsAsync(BehaviorId behaviorId, Action<List<EventHolder>> updateHandler)
        //     => Cache.UpdateAsync(
        //         $"Stateflows.Notifications.{behaviorId.ToString()}",
        //         value =>
        //         {
        //             var notifications =
        //                 StateflowsJsonConverter.DeserializeObject<List<EventHolder>>(value);
        //
        //             if (notifications != null)
        //             {
        //                 updateHandler(notifications);
        //
        //                 value = StateflowsJsonConverter.SerializePolymorphicObject(notifications);
        //             }
        //
        //             return value;
        //         },
        //         StateflowsJsonConverter.SerializePolymorphicObject(new List<EventHolder>())
        //     );
        
        // private async Task<IEnumerable<EventHolder>> GetNotificationsAsync(BehaviorId behaviorId, Func<EventHolder, bool> filter = null)
        // {
        //     List<EventHolder> notifications;
        //
        //     var (result, value) = await Cache.TryGetAsync($"Stateflows.Notifications.{behaviorId.ToString()}");
        //
        //     if (!result)
        //     {
        //         return Array.Empty<EventHolder>();
        //     }
        //     
        //     notifications = StateflowsJsonConverter.DeserializeObject<List<EventHolder>>(value);
        //
        //     return (
        //         filter == null
        //             ? notifications
        //             : notifications.Where(filter)
        //     ).ToArray();
        // }

        public Task<IEnumerable<EventHolder>> GetNotificationsAsync(BehaviorId behaviorId, string[] notificationNames,
            DateTime? lastNotificationCheck = null)
            => Storage.GetNotificationsAsync(behaviorId, notificationNames, lastNotificationCheck ?? DateTime.Now);
            // => GetNotificationsAsync(behaviorId, h =>
            //     notificationNames.Contains(h.Name) &&
            //     (
            //         h.SentAt.AddSeconds(h.TimeToLive) >= (lastNotificationCheck ?? DateTime.Now) ||
            //         h.Retained
            //     )
            // );

        public async Task<IEnumerable<TNotification>> GetNotificationsAsync<TNotification>(BehaviorId behaviorId, DateTime? lastNotificationCheck = null)
            => (await Storage.GetNotificationsAsync(behaviorId, new string[] { Event<TNotification>.Name }, lastNotificationCheck ?? DateTime.Now))
                .Select(n => ((EventHolder<TNotification>)n).Payload);
            // => (
            //         await GetNotificationsAsync(behaviorId, h =>
            //             h is EventHolder<TNotification> &&
            //             (
            //                 h.SentAt.AddSeconds(h.TimeToLive) >= (lastNotificationCheck ?? DateTime.Now) ||
            //                 h.Retained
            //             )
            //         )
            //     )
            //     .Select(h => h as EventHolder<TNotification>)
            //     .Select(h => h!.Payload);

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

        private Task RunHandlersAsync(EventHolder eventHolder)
        {
            INotificationHandler[] handlers;
            lock (Handlers)
            {
                handlers = Handlers.ToArray();
            }

            return Task.WhenAll(handlers.Select(handler => handler.HandleNotificationAsync(eventHolder)));
        }   

        public Task PublishAsync(EventHolder eventHolder)
            => PublishRangeAsync(new EventHolder[] { eventHolder });

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

                // await UpdateNotificationsAsync(
                //     group.Key,
                //     notifications =>
                //     {
                //         var retainedNotifications = group.Where(n => n.Retained).Select(n => n.Name);
                //         foreach (var notification in notifications.Where(n => retainedNotifications.Contains(n.Name)))
                //         {
                //             notification.Retained = false;
                //         }
                //         notifications.AddRange(group);
                //     });
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // _ = Task.Run(() => TimingLoop(CancellationTokenSource.Token), cancellationToken);

            return Task.CompletedTask;
        }

        private static DateTime GetCurrentTick()
            => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

        // private async Task TimingLoop(CancellationToken cancellationToken)
        // {
        //     var lastTick = GetCurrentTick();
        //
        //     while (!cancellationToken.IsCancellationRequested)
        //     {
        //         var diffInSeconds = (DateTime.Now - lastTick).TotalSeconds;
        //
        //         if (diffInSeconds >= 60)
        //         {
        //             lastTick = GetCurrentTick();
        //
        //             var date = DateTime.Now.AddMinutes(-1);
        //             // await UpdateNotificationsAsync(notifications =>
        //             // {
        //             //     foreach (var behaviorNotifications in notifications.Values)
        //             //     {
        //             //         behaviorNotifications.RemoveAll(notification => notification.SentAt.AddSeconds(notification.TimeToLive) <= date);
        //             //     }
        //             //     
        //             //     var emptyIds = notifications
        //             //         .Where(x => !x.Value.Any())
        //             //         .Select(x => x.Key)
        //             //         .ToArray();
        //             //     
        //             //     foreach (var id in emptyIds)
        //             //     {
        //             //         notifications.Remove(id);
        //             //     }
        //             // });
        //         }
        //
        //         await Task.Delay(1000, cancellationToken);
        //     }
        // }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            CancellationTokenSource.Cancel();

            return Task.CompletedTask;
        }
    }
}
