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
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        private readonly Dictionary<BehaviorId, List<EventHolder>> notifications = new Dictionary<BehaviorId, List<EventHolder>>();

        public Dictionary<BehaviorId, List<EventHolder>> Notifications
            => notifications;

        public event Action<EventHolder> OnPublish;

        public Task PublishAsync(EventHolder eventHolder)
            => PublishRangeAsync(new EventHolder[] { eventHolder });

        public Task PublishRangeAsync(IEnumerable<EventHolder> eventHolders)
        {
            var holdersBySenderIds = eventHolders
                .Where(h => h.SenderId != null)
                .GroupBy(h => (BehaviorId)h.SenderId);

            lock (Notifications)
            {
                foreach (var group in holdersBySenderIds)
                {
                    if (!Notifications.TryGetValue(group.Key, out var behaviorNotifications))
                    {
                        behaviorNotifications = new List<EventHolder>();
                        Notifications.Add(group.Key, behaviorNotifications);
                    }

                    behaviorNotifications.AddRange(group);
                }
            }

            var tasks = eventHolders.Select(h => Task.Run(() => OnPublish.Invoke(h)));
            _ = Task.WhenAll(tasks);

            return Task.CompletedTask;
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
                        foreach (var behaviorNotifications in Notifications.Values)
                        {
                            behaviorNotifications.RemoveAll(notification => notification.SentAt <= date);
                        }

                        var emptyIds = Notifications
                            .Where(x => !x.Value.Any())
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
