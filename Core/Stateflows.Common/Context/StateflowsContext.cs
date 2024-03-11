using System;
using System.Linq;
using System.Collections.Generic;

namespace Stateflows.Common.Context
{
    public class StateflowsContext
    {
        public BehaviorId Id { get; set; }

        public int Version { get; set; } = 0;

        public BehaviorStatus Status { get; set; } = BehaviorStatus.Unknown;

        public DateTime LastExecutedAt { get; set; }

        public DateTime? TriggerTime { get; set; }

        public bool ShouldSerializePendingTimeEvents()
            => PendingTimeEvents.Any();

        public Dictionary<Guid, TimeEvent> PendingTimeEvents { get; set; } = new Dictionary<Guid, TimeEvent>();

        public bool ShouldSerializeValues()
            => Values.Any();

        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

        public bool ShouldSerializeGlobalValues()
            => GlobalValues.Any();

        public Dictionary<string, string> GlobalValues { get; } = new Dictionary<string, string>();
    }
}
