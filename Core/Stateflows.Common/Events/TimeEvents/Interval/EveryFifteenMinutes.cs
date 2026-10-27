using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring event every fifteen minutes, aligned to quarter-hour boundaries.</summary>
    public sealed class EveryFifteenMinutes : IntervalEvent
    {
        /// <inheritdoc />
        protected override DateTime GetIntervalStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, startedAt.Minute - (startedAt.Minute % 15), 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected sealed override TimeSpan Interval => new TimeSpan(0, 15, 0);
    }
}
