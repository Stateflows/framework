using System.Net;
using System.Net.Http.Json;
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

        public async Task<SendResult> SendAsync(BehaviorId behaviorId, Event @event)
        {
            var requestResult = await _httpClient.PostAsJsonAsync(
                "/stateflows/send",
                new StateflowsRequest() { Event = @event, BehaviorId = behaviorId }
            );
            if (requestResult.StatusCode == HttpStatusCode.OK)
            {
                var result = await requestResult.Content.ReadFromJsonAsync<StateflowsResponse>();
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

        public async Task<IEnumerable<BehaviorClass>> GetAvailableClassesAsync()
            => await _httpClient.GetFromJsonAsync<IEnumerable<BehaviorClass>>($"/stateflows/availableClasses") ?? Array.Empty<BehaviorClass>();
    }
}