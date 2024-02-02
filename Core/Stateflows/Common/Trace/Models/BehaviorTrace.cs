using Stateflows.Common.Context;

namespace Stateflows.Common.Trace.Models
{
    public class BehaviorTrace : BehaviorTraceStep
    {
        public BehaviorId BehaviorId { get; set; }

        public Event Event { get; set; }

        public StateflowsContext Context { get; set; }
    }
}
