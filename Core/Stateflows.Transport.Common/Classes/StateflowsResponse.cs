using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stateflows.Common.Transport.Classes
{
    public class StateflowsResponse
    {
        public EventStatus EventStatus { get; set; }

        public EventValidation Validation { get; set; }

        public Response Response { get; set; }

        public DateTime ResponseTime { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<EventHolder> Notifications { get; set; } = Array.Empty<EventHolder>();
    }
}
