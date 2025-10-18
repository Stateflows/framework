using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;

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

        public TreeNode<string> HistoryTree { get; set; }

        public bool ShouldSerializeHistoryTree()
            => HistoryTree != null;
        
        public Dictionary<string, Guid> TimeEventIds { get; set; } = new Dictionary<string, Guid>();

        public bool ShouldSerializeStartupEventIds()
            => StartupEventIds.Any();

        public Dictionary<string, Guid> StartupEventIds { get; set; } = new Dictionary<string, Guid>();
    }
}
