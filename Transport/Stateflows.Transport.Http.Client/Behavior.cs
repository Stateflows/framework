using Stateflows.Utils;
using Stateflows.Common;
using Stateflows.Common.Classes;
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

            return apiClient.SendAsync(Id, @event.ToEventHolder(@event.GetType()), watches);
        }

        public async Task<RequestResult<TResponseEvent>> RequestAsync<TResponseEvent>(IRequest<TResponseEvent> request, IEnumerable<EventHeader> headers = null)
        {
            ResponseHolder.SetResponses(new Dictionary<object, EventHolder>());

            var result = await SendAsync(request);
            return new RequestResult<TResponseEvent>(request.ToEventHolder(request.GetType()), result.Status, result.Validation);
        }

        public Task<IWatcher> WatchAsync<TNotificationEvent>(Action<TNotificationEvent> handler)
        {
            lock (watches)
            {
                var notificationName = Event<TNotificationEvent>.Name;
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

                watch.Handlers.Add(notification => handler((TNotificationEvent)notification.BoxedPayload));
            }

            return Task.FromResult((IWatcher)new Watcher<TNotificationEvent>(this));
        }

        public Task UnwatchAsync<TNotificationEvent>()
        {
            lock (watches)
            {
                var notificationName = Event<TNotificationEvent>.Name;
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
