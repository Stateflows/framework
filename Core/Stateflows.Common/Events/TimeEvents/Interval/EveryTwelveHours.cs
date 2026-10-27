using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring event every twelve hours, aligned to twelve-hour boundaries.</summary>
    public sealed class EveryTwelveHours : IntervalEvent
    {
        /// <inheritdoc />
        protected override DateTime GetIntervalStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour - (startedAt.Hour % 12), 0, 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected sealed override TimeSpan Interval => new TimeSpan(12, 0, 0);
    }
}
