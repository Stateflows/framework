using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring event weekly on a configured weekday at UTC midnight.</summary>
    public class EveryOneWeek : IntervalEvent
    {
        /// <summary>Gets the weekday used to align the weekly schedule; defaults to Monday.</summary>
        protected virtual DayOfWeek Day => DayOfWeek.Monday;

        /// <inheritdoc />
        protected override DateTime GetIntervalStart(DateTime startedAt)
        {
            var result = new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, 0, 0, 0, DateTimeKind.Utc);

            return result.AddDays(Day - result.DayOfWeek);
        }

        /// <inheritdoc />
        protected sealed override TimeSpan Interval => new TimeSpan(7, 0, 0, 0);
    }
}
