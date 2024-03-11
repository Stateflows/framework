using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Subscription
{
    internal class NotificationsHub : IHostedService, INotificationsHub
    {
        private readonly Dictionary<BehaviorId, List<Notification>> Notifications = new Dictionary<BehaviorId, List<Notification>>();
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public event Action<Notification> OnPublish;

        public Task PublishAsync<TNotification>(TNotification notification)
            where TNotification : Notification, new()
        {
            lock (Notifications)
            {
                if (!Notifications.TryGetValue(notification.SenderId, out var notifications))
                {
                    notifications = new List<Notification>();
                    Notifications.Add(notification.SenderId, notifications);
                }

                notifications.Add(notification);
            }

            _ = Task.Run(() => OnPublish.Invoke(notification));

            return Task.CompletedTask;
        }

        public Notification[] GetPendingNotifications(BehaviorId behaviorId, DateTime notificationThreshold)
        {
            lock (Notifications)
            {
                return Notifications.TryGetValue(behaviorId, out var notifications)
                    ? notifications
                        .Where(notification => notification.SentAt >= notificationThreshold)
                        .ToArray()
                    : Array.Empty<Notification>();
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(() => TimingLoop(CancellationTokenSource.Token));

            return Task.CompletedTask;
        }

        private DateTime GetCurrentTick()
            => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

        private async Task TimingLoop(CancellationToken cancellationToken)
        {
            var lastTick = GetCurrentTick();

            while (!cancellationToken.IsCancellationRequested)
            {
                var diffInSeconds = (DateTime.Now - lastTick).TotalSeconds;

                if (diffInSeconds >= 60)
                {
                    lastTick = GetCurrentTick();

                    lock (Notifications)
                    {
                        var date = DateTime.Now.AddMinutes(-1);
                        foreach (var notifications in Notifications.Values)
                        {
                            notifications.RemoveAll(notification => notification.SentAt <= date);
                        }

                        var emptyIds = Notifications
                            .Where(x => x.Value.Any())
                            .Select(x => x.Key)
                            .ToArray();

                        foreach (var id in emptyIds)
                        {
                            Notifications.Remove(id);
                        }
                    }
                }

                await Task.Delay(1000);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            CancellationTokenSource.Cancel();

            return Task.CompletedTask;
        }
    }
}
