using System;

namespace Stateflows.Common
{
    public class EveryOneDay : IntervalEvent
    {
        protected virtual DateTime TimeOfDay => new DateTime();

        protected sealed override DateTime GetIntervalStart(DateTime startedAt)
        {
            var time = TimeOfDay;
            return new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, time.Hour, time.Minute, time.Second);
        }

        protected sealed override TimeSpan Interval => new TimeSpan(1, 0, 0, 0);
    }
}
