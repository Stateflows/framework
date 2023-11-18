using System;

namespace Stateflows.Common
{
    public sealed class EveryTwelveHours : IntervalEvent
    {
        protected override DateTime GetIntervalStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour - (startedAt.Hour % 12), 0, 0);

        protected override sealed TimeSpan Interval => new TimeSpan(12, 0, 0);
    }
}
