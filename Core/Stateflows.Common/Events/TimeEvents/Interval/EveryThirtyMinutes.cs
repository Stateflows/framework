using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring event every thirty minutes, aligned to half-hour boundaries.</summary>
    public sealed class EveryThirtyMinutes : IntervalEvent
    {
        /// <inheritdoc />
        protected override DateTime GetIntervalStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, startedAt.Minute - (startedAt.Minute % 30), 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected sealed override TimeSpan Interval => new TimeSpan(0, 30, 0);
    }
}
