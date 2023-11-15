using System;

namespace Stateflows.Common
{
    public abstract class IntervalEvent : RecurringEvent
    {
        protected abstract TimeSpan Interval { get; }

        protected virtual DateTime GetIntervalStart(DateTime startedAt)
            => startedAt;

        public override sealed bool ShouldTrigger(DateTime startedAt)
            => DateTime.Now >= GetIntervalStart(startedAt).Add(Interval);
    }
}
