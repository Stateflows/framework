using System.Collections.Generic;

namespace Stateflows.Common.Context
{
    public class StateflowsContext
    {
        public BehaviorId Id { get; set; }

        public int Version { get; set; } = 0;

        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

        public Dictionary<string, string> GlobalValues { get; } = new Dictionary<string, string>();
    }
}
