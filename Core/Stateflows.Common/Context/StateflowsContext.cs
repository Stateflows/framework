using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stateflows.Common.Context
{
    public class StateflowsContext
    {
        public BehaviorId Id { get; set; }

        public int Version { get; set; } = 0;

        public BehaviorStatus Status { get; set; } = BehaviorStatus.Unknown;

        public DateTime LastExecutedAt { get; set; }

        public DateTime? TriggerTime { get; set; }

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
        public Dictionary<Guid, TimeEvent> PendingTimeEvents { get; set; } = new Dictionary<Guid, TimeEvent>();

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

        public Dictionary<string, string> GlobalValues { get; } = new Dictionary<string, string>();
    }
}
