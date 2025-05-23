using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Transport.Classes;
using Stateflows.Common.Transport.Interfaces;

namespace Stateflows.Transport.Http.Client
{
    internal class Behavior : IBehavior, IUnwatcher, INotificationTarget
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

        private void ApiClient_OnNotify(EventHolder notificationHolder, DateTime responseTime)
        {
            lock (watches)
            {
                foreach (var watch in watches)
                {
                    watch.LastNotificationCheck = responseTime;
                }

                var notifiedWatch = watches.Find(watch => watch.NotificationName == notificationHolder.Name);
                if (notifiedWatch != null && !notifiedWatch.Notifications.Any(n => n.Id == notificationHolder.Id))
                {
                    notifiedWatch.Notifications.Add(notificationHolder);
                    Task.Run(() =>
                    {
                        foreach (var handler in notifiedWatch.Handlers)
                        {
                            handler.Invoke(notificationHolder);
                        }
                    });
                }
            }
        }

        public Task<SendResult> SendAsync<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)
        {
            ResponseHolder.SetResponses(new Dictionary<object, EventHolder>());

            var eventHolder = @event.ToTypedEventHolder(headers);

            return apiClient.SendAsync(Id, eventHolder, watches);
        }

        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(IRequest<TResponse> request, IEnumerable<EventHeader> headers = null)
        {
            ResponseHolder.SetResponses(new Dictionary<object, EventHolder>());

            var eventHolder = request.ToTypedEventHolder(headers);

            var result = await SendAsync(request);
            return new RequestResult<TResponse>(eventHolder, result.Status, result.Notifications, result.Validation);
        }

        public Task<IWatcher> WatchAsync<TNotification>(Action<TNotification> handler)
        {
            lock (watches)
            {
                var notificationName = Event<TNotification>.Name;
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

                watch.Handlers.Add(notification => handler((TNotification)notification.BoxedPayload));
            }

            return Task.FromResult<IWatcher>(new Watcher<TNotification>(this));
        }

        public Task UnwatchAsync<TNotification>()
        {
            lock (watches)
            {
                var notificationName = Event<TNotification>.Name;
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
            lock (watches)
            {
                watches.Clear();
            }

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
