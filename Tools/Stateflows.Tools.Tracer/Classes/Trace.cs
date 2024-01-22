using System;
using Stateflows.Common;
using Stateflows.Common.Context;

namespace Stateflows.Tools.Tracer.Classes
{
    internal abstract class Trace
    {
        public DateTime ExecutedAt { get; set; }

        public TimeSpan Duration { get; set; }

        public BehaviorId BehaviorId { get; set; }

        public Event? Event { get; set; }

        public StateflowsContext? Context { get; set; }
    }
}
