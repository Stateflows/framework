using System;
using Newtonsoft.Json;

namespace Stateflows.Tools.Tracer.StateMachines.Models
{
    internal class Step
    {
        public DateTime ExecutedAt { get; set; }

        public TimeSpan Duration { get; set; }

        public string? Action { get; set; }

        public string? Element { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Exception? Exception { get; set; }
    }
}
