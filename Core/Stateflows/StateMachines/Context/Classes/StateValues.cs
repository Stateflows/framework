using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.StateMachines.Context.Classes
{
    /// <summary>Persisted state-local values, embedded behavior identity, remembered configuration, and scheduled event identifiers.</summary>
    public class StateValues
    {
        /// <summary>Determines whether state-local serialized values should be included in persistence.</summary>
        /// <returns>Whether the value dictionary is nonempty.</returns>
        public bool ShouldSerializeValues()
            => Values.Any();

        /// <summary>Serialized values keyed by their state-local names.</summary>
        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();

        /// <summary>Determines whether an embedded behavior identity should be persisted.</summary>
        /// <returns>Whether an embedded behavior identity is assigned.</returns>
        public bool ShouldSerializeBehaviorId()
            => BehaviorId != null;

        /// <summary>The behavior instance embedded in this state, when one is assigned.</summary>
        public BehaviorId? BehaviorId { get; set; } = null;

        /// <summary>Determines whether scheduled time-event identifiers should be persisted.</summary>
        /// <returns>Whether any scheduled time events are tracked.</returns>
        public bool ShouldSerializeTimeEventIds()
            => TimeEventIds.Any();

        /// <summary>The remembered active-state configuration used by history restoration.</summary>
        public TreeNode<string> HistoryTree { get; set; }

        /// <summary>Determines whether a remembered configuration should be persisted.</summary>
        /// <returns>Whether the history tree is assigned.</returns>
        public bool ShouldSerializeHistoryTree()
            => HistoryTree != null;
        
        /// <summary>Identifiers of scheduled time events keyed by their registration names.</summary>
        public Dictionary<string, Guid> TimeEventIds { get; set; } = new Dictionary<string, Guid>();

        /// <summary>Determines whether startup-event identifiers should be persisted.</summary>
        /// <returns>Whether any startup events are tracked.</returns>
        public bool ShouldSerializeStartupEventIds()
            => StartupEventIds.Any();

        /// <summary>Identifiers of startup events keyed by their registration names.</summary>
        public Dictionary<string, Guid> StartupEventIds { get; set; } = new Dictionary<string, Guid>();
    }
}
