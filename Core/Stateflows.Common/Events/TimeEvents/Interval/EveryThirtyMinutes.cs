using System;

namespace Stateflows.Common
{
    public sealed class EveryThirtyMinutes : IntervalEvent
    {
        protected override DateTime GetIntervalStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, startedAt.Minute - (startedAt.Minute % 30), 0);

        protected override sealed TimeSpan Interval => new TimeSpan(0, 30, 0);
    }
}
