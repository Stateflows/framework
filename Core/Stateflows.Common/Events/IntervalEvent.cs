using System;

namespace Stateflows.Common
{
    public abstract class IntervalEvent : TimeEvent
    {
        protected abstract TimeSpan Interval { get; }

        public override sealed bool ShouldTrigger(DateTime startedAt)
            => DateTime.Now >= startedAt.Add(Interval);
    }
}
