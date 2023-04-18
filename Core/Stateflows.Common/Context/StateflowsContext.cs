using System.Collections.Generic;

namespace Stateflows.Common.Context
{
    public class StateflowsContext
    {
        public BehaviorId Id { get; set; }

        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();
    }
}
