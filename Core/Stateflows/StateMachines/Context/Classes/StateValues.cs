using System;
using System.Linq;
using System.Collections.Generic;

namespace Stateflows.StateMachines.Context.Classes
{
    public class StateValues
    {
        public bool ShouldSerializeValues()
            => Values.Any();

        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();

        public bool ShouldSerializeBehaviorId()
            => BehaviorId != null;

        public BehaviorId? BehaviorId { get; set; } = null;

        public bool ShouldSerializeTimeEventIds()
            => TimeEventIds.Any();

        public Dictionary<string, Guid> TimeEventIds { get; set; } = new Dictionary<string, Guid>();
    }
}
