using Stateflows.Common;
using Stateflows.Common.Transport.Classes;

namespace Stateflows.Transport.Http.Client
{
    internal class Behavior : IBehavior
    {
        private readonly StateflowsApiClient apiClient;
        private readonly List<Watch> Watches = new List<Watch>();

        public BehaviorId Id { get; }

        public Behavior(StateflowsApiClient apiClient, BehaviorId id)
        {
            this.apiClient = apiClient;
            Id = id;
        }

        public async Task<SendResult> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
            => await apiClient.SendAsync(Id, @event, Watches);

        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            var result = await SendAsync(request as Event);
            return new RequestResult<TResponse>(request, result.Status, result.Validation);
        }

        public Task WatchAsync<TNotification>(Action<TNotification> handler)
            where TNotification : Notification, new()
        {
            var notificationName = EventInfo<TNotification>.Name;
            if (!Watches.Any(watch => watch.NotificationName == notificationName))
            {
                Watches.Add(
                    new Watch()
                    {
                        LastNotificationCheck = DateTime.Now,
                        NotificationName = notificationName
                    }
                );
            }

            return Task.CompletedTask;
        }

        public Task UnwatchAsync<TNotification>()
            where TNotification : Notification, new()
        {
            var notificationName = EventInfo<TNotification>.Name;
            var watch = Watches.Find(watch => watch.NotificationName == notificationName);
            if (watch != null)
            {
                Watches.Remove(watch);
            }

            return Task.CompletedTask;
        }
    }
}
