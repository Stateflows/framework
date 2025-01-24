using System;

namespace Stateflows.Common
{
    public sealed class EveryFiveMinutes : IntervalEvent
    {
        protected override DateTime GetIntervalStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, startedAt.Minute - (startedAt.Minute % 5), 0);

        protected sealed override TimeSpan Interval => new TimeSpan(0, 5, 0);
    }
}
