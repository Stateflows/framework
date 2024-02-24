using System.Text;
using System.Net;
using System.Net.Mime;
using System.Net.Http.Json;
using System.Diagnostics;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Extensions;
using Stateflows.Common.Transport.Classes;

namespace Stateflows.Transport.Http.Client
{
    public class StateflowsApiClient
    {
        private readonly HttpClient _httpClient;

        public StateflowsApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [DebuggerHidden]
        public async Task<SendResult> SendAsync(BehaviorId behaviorId, Event @event)
        {
            var requestResult = await _httpClient.PostAsync(
                "/stateflows/send",
                new StringContent(
                    StateflowsJsonConverter.SerializePolymorphicObject(
                        new StateflowsRequest() { Event = @event, BehaviorId = behaviorId }
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