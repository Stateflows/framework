using System.Net;
using System.Text;
using System.Net.Mime;
using System.Net.Http.Json;
using System.Diagnostics;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Extensions;
using Stateflows.Common.Transport.Classes;
using Stateflows.Common.Transport.Interfaces;

namespace Stateflows.Transport.Http.Client
{
    internal class StateflowsApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        private readonly Timer _timer;

        public event Action<Notification, DateTime>? OnNotify;

        public List<INotificationTarget> NotificationTargets { get; } = new();

        public StateflowsApiClient(HttpClient httpClient, StateflowsApiClientConfig config)
        {
            _httpClient = httpClient;

            _timer = new Timer(async (_) => await this.CheckForNotificationsAsync(), null, 0, 1000 * config.NotificationsCheckSecondsInverval);
        }

        private async Task CheckForNotificationsAsync()
        {
            IEnumerable<INotificationTarget> targets;
            lock (this)
            {
                targets = NotificationTargets;
            }

            await Task.WhenAll(targets.Select(target => SendAsync(target.Id, new NotificationsRequest(), target.Watches)));
        }

        [DebuggerHidden]
        public async Task<SendResult<TEvent>> SendAsync<TEvent>(BehaviorId behaviorId, TEvent @event, IEnumerable<Watch> watches)
        {
            var requestResult = await _httpClient.PostAsync(
                "/stateflows/send",
                new StringContent(
                    StateflowsJsonConverter.SerializePolymorphicObject(
                        new StateflowsRequest()
                        {
                            Event = @event,
                            BehaviorId = behaviorId,
                            Watches = watches
                        },
                        true
                    ),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json
                )
            );

            if (requestResult.StatusCode == HttpStatusCode.OK)
            {
                var jsonString = await requestResult.Content.ReadAsStringAsync();
                var result = StateflowsJsonConverter.DeserializeObject<StateflowsResponse>(jsonString);
                if (result != null)
                {
                    if (result.Response != null && @event is Request request)
                    {
                        request.Respond(result.Response);
                    }

                    lock (this)
                    {
                        foreach (var notification in result.Notifications)
                        {
                            OnNotify?.Invoke(notification, result.ResponseTime);
                        }
                    }

                    return new SendResult<TEvent>(@event, result.EventStatus, result.Validation);
                }
            }

            return new SendResult<TEvent>(@event, EventStatus.Rejected);
        }

        [DebuggerHidden]
        public async Task<IEnumerable<BehaviorClass>> GetAvailableClassesAsync()
            => await _httpClient.GetFromJsonAsync<IEnumerable<BehaviorClass>>($"/stateflows/availableClasses") ?? Array.Empty<BehaviorClass>();


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
            }
        }
    }
}