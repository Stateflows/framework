using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring event daily at a configured UTC time of day.</summary>
    public class EveryOneDay : IntervalEvent
    {
        /// <summary>Gets the time of day used to align the daily schedule.</summary>
        protected virtual DateTime TimeOfDay => new DateTime();

        /// <inheritdoc />
        protected sealed override DateTime GetIntervalStart(DateTime startedAt)
        {
            var time = TimeOfDay;
            return new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, time.Hour, time.Minute, time.Second, DateTimeKind.Utc);
        }

        /// <inheritdoc />
        protected sealed override TimeSpan Interval => new TimeSpan(1, 0, 0, 0);
    }
}
