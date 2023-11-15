using System;

namespace Stateflows.Common
{
    public sealed class EveryOneMonth : IntervalEvent
    {
        protected override sealed DateTime GetIntervalStart(DateTime startedAt)
        {
            var result = new DateTime(startedAt.Year, startedAt.Month, 1);
            result.AddMonths(1);
            return result;
        }

        protected override sealed TimeSpan Interval => TimeSpan.Zero;
    }
}
