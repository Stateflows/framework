using Newtonsoft.Json;
using System.Collections.Generic;

namespace Stateflows.Common
{
    [NoImplicitInitialization, NoTracing]
    public sealed class StopRelay
    {
        public BehaviorId BehaviorId { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public List<string> NotificationNames { get; set; } = new List<string>();
    }
}
