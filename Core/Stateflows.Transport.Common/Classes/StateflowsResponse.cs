﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stateflows.Common.Transport.Classes
{
    public class StateflowsResponse
    {
        public EventStatus EventStatus { get; set; }

        public string EventStatusText => EventStatus.ToString();

        public EventValidation Validation { get; set; }

        public EventHolder Response { get; set; }

        public DateTime ResponseTime { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<EventHolder> Notifications { get; set; } = Array.Empty<EventHolder>();
    }
}
