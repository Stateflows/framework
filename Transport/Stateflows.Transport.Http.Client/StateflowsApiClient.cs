using System.Net;
using System.Text;
using System.Net.Mime;
using System.Net.Http.Json;
using System.Threading;
using System.Diagnostics;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Extensions;
using Stateflows.Common.Transport.Classes;
using System.Runtime.CompilerServices;

namespace Stateflows.Transport.Http.Client
{
    public class StateflowsApiClient
    {
        private readonly HttpClient _httpClient;

        private readonly Timer _timer;

        public event Action<Notification> OnNotify;

        public StateflowsApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _timer = new Timer((_) =>
            {

            }, null, 0, 1000 * 10);

        }

[DebuggerHidden]
        public async Task<SendResult> SendAsync(BehaviorId behaviorId, Event @event, IEnumerable<Watch> watches)
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
                        }
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
                    if (result.Response != null)
                    {
                        @event.Respond(result.Response);
                    }

                    foreach (var notification in result.Notifications)
                    {
                        OnNotify?.Invoke(notification);
                    }

                    return new SendResult(@event, result.EventStatus, result.Validation);
                }
            }

            return new SendResult(@event, EventStatus.Rejected);
        }

        [DebuggerHidden]
        public async Task<IEnumerable<BehaviorClass>> GetAvailableClassesAsync()
            => await _httpClient.GetFromJsonAsync<IEnumerable<BehaviorClass>>($"/stateflows/availableClasses") ?? Array.Empty<BehaviorClass>();
    }
}