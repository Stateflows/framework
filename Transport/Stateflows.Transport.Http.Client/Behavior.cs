using Stateflows.Common;
using Stateflows.Common.Transport.Classes;
using Stateflows.Common.Transport.Interfaces;

namespace Stateflows.Transport.Http.Client
{
    internal class Behavior : IBehavior, INotificationTarget
    {
        private readonly StateflowsApiClient apiClient;
        private readonly List<Watch> watches = new();
        public IEnumerable<Watch> Watches
            => watches;

        public BehaviorId Id { get; }

        public Behavior(StateflowsApiClient apiClient, BehaviorId id)
        {
            this.apiClient = apiClient;
            lock (apiClient)
            {
                this.apiClient.OnNotify += ApiClient_OnNotify;
                this.apiClient.NotificationTargets.Add(this);
            }

            Id = id;
        }

        private void ApiClient_OnNotify(Notification notification, DateTime responseTime)
        {
            lock (watches)
            {
                foreach (var watch in watches)
                {
                    watch.LastNotificationCheck = responseTime;
                }

                var notifiedWatch = watches.Find(watch => watch.NotificationName == notification.Name);
                if (notifiedWatch != null && !notifiedWatch.Notifications.Any(n => n.Id == notification.Id))
                {
                    notifiedWatch.Notifications.Add(notification);
                    Task.Run(() =>
                    {
                        foreach (var handler in notifiedWatch.Handlers)
                        {
                            handler.Invoke(notification);
                        }
                    });
                }
            }
        }

        public async Task<SendResult> SendAsync<TEvent>(TEvent @event)
            => await apiClient.SendAsync(Id, @event, watches);

        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : IResponse, new()
        {
            var result = await SendAsync(request as EventHolder);
            return new RequestResult<TResponse>(request, result.Status, result.Validation);
        }

        public Task WatchAsync<TNotification>(Action<TNotification> handler)
            where TNotification : Notification, new()
        {
            lock (watches)
            {
                var notificationName = EventInfo<TNotification>.Name;
                var watch = watches.Find(watch => watch.NotificationName == notificationName);
                if (watch == null)
                {
                    watch = new Watch()
                    {
                        LastNotificationCheck = DateTime.Now,
                        NotificationName = notificationName
                    };

                    watches.Add(watch);
                }

                watch.Handlers.Add(notification => handler((TNotification)notification));
            }

            return Task.CompletedTask;
        }

        public Task UnwatchAsync<TNotification>()
            where TNotification : Notification, new()
        {
            lock (watches)
            {
                var notificationName = EventInfo<TNotification>.Name;
                var watch = watches.Find(watch => watch.NotificationName == notificationName);
                if (watch != null)
                {
                    watches.Remove(watch);
                }
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            watches.Clear();
            lock (apiClient)
            {
                apiClient.OnNotify -= ApiClient_OnNotify;
                apiClient.NotificationTargets.Remove(this);
            }
        }

        ~Behavior()
            => Dispose(false);
    }
}
