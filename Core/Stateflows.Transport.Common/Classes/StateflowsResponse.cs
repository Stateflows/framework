using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Stateflows.Common.Transport.Classes
{
    public class StateflowsResponse
    {
        public EventStatus EventStatus { get; set; }

        public EventValidation Validation { get; set; }

        public Response Response { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<Notification> Notifications { get; set; } = Array.Empty<Notification>();
    }
}
