using System;

namespace Stateflows.Common
{
    public class EveryOneWeek : IntervalEvent
    {
        protected virtual DayOfWeek Day => DayOfWeek.Monday;

        protected override DateTime GetIntervalStart(DateTime startedAt)
        {
            var result = new DateTime(startedAt.Year, startedAt.Month, startedAt.Day);

            return result.AddDays(Day - result.DayOfWeek);
        }

        protected sealed override TimeSpan Interval => new TimeSpan(7, 0, 0, 0);
    }
}
