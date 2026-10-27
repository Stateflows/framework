using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring event every hour, aligned to whole-hour boundaries.</summary>
    public sealed class EveryOneHour : IntervalEvent
    {
        /// <inheritdoc />
        protected override DateTime GetIntervalStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, 0, 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected sealed override TimeSpan Interval => new TimeSpan(1, 0, 0);
    }
}
