using System;

namespace Stateflows.Common
{
    public sealed class EveryOneMonth : IntervalEvent
    {
        protected sealed override DateTime GetIntervalStart(DateTime startedAt)
        {
            var result = new DateTime(startedAt.Year, startedAt.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            result.AddMonths(1);
            return result;
        }

        protected sealed override TimeSpan Interval => TimeSpan.Zero;
    }
}
