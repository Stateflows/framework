using System;

namespace Stateflows.Common
{
    public sealed class EveryOneHour : IntervalEvent
    {
        protected override DateTime GetIntervalStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, 0, 0);

        protected sealed override TimeSpan Interval => new TimeSpan(1, 0, 0);
    }
}
