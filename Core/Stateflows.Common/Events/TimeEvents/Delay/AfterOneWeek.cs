using System;

namespace Stateflows.Common
{
    /// <summary>Schedules an event one week after the most recent configured weekday at UTC midnight.</summary>
    public class AfterOneWeek : DelayEvent
    {
        /// <summary>Gets the weekday used to align the delay start; defaults to Monday.</summary>
        protected virtual DayOfWeek Day => DayOfWeek.Monday;

        /// <inheritdoc />
        protected override DateTime GetDelayStart(DateTime startedAt)
        {
            var result = new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, 0, 0, 0, DateTimeKind.Utc);

            return result.AddDays(Day - result.DayOfWeek);
        }

        /// <inheritdoc />
        protected override TimeSpan Delay => new TimeSpan(7, 0, 0, 0);
    }
}
