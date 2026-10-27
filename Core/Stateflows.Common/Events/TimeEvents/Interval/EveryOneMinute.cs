using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring event every minute, aligned to minute boundaries.</summary>
    public sealed class EveryOneMinute : IntervalEvent
    {
        /// <inheritdoc />
        protected override DateTime GetIntervalStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, startedAt.Minute, 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected sealed override TimeSpan Interval => new TimeSpan(0, 1, 0);
    }
}
