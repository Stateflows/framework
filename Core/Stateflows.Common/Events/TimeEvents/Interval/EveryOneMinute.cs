using System;

namespace Stateflows.Common
{
    public sealed class EveryOneMinute : IntervalEvent
    {
        protected override DateTime GetIntervalStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, startedAt.Minute, 0);

        protected override sealed TimeSpan Interval => new TimeSpan(0, 1, 0);
    }
}
