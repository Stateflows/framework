using System;

namespace Stateflows.Common
{
    public class AfterOneWeek : DelayEvent
    {
        protected virtual DayOfWeek Day => DayOfWeek.Monday;

        protected override DateTime GetDelayStart(DateTime startedAt)
        {
            var result = new DateTime(startedAt.Year, startedAt.Month, startedAt.Day);

            return result.AddDays(Day - result.DayOfWeek);
        }

        protected override TimeSpan Delay => new TimeSpan(7, 0, 0, 0);
    }
}
