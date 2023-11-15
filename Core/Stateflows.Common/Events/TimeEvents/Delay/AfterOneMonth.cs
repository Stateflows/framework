using System;

namespace Stateflows.Common
{
    public sealed class AfterOneMonth : DelayEvent
    {
        protected override sealed DateTime GetDelayStart(DateTime startedAt)
        {
            var result = new DateTime(startedAt.Year, startedAt.Month, 1);
            result.AddMonths(1);
            return result;
        }

        protected override sealed TimeSpan Delay => TimeSpan.Zero;
    }
}
