using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring event every five minutes, aligned to five-minute boundaries.</summary>
    public sealed class EveryFiveMinutes : IntervalEvent
    {
        /// <inheritdoc />
        protected override DateTime GetIntervalStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, startedAt.Minute - (startedAt.Minute % 5), 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected sealed override TimeSpan Interval => new TimeSpan(0, 5, 0);
    }
}
