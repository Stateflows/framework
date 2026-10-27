using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring event every ten minutes, aligned to ten-minute boundaries.</summary>
    public sealed class EveryTenMinutes : IntervalEvent
    {
        /// <inheritdoc />
        protected override DateTime GetIntervalStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, startedAt.Minute - (startedAt.Minute % 10), 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected sealed override TimeSpan Interval => new TimeSpan(0, 10, 0);
    }
}
