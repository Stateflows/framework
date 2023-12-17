using System;

namespace Stateflows.Common
{
    public abstract class IntervalEvent : RecurringEvent
    {
        protected abstract TimeSpan Interval { get; }

        protected virtual DateTime GetIntervalStart(DateTime startedAt)
            => startedAt;

        protected override DateTime GetTriggerTime(DateTime startedAt)
            => GetIntervalStart(startedAt).Add(Interval);
    }
}
