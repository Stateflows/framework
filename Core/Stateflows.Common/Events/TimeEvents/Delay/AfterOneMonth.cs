using System;

namespace Stateflows.Common
{
    public sealed class AfterOneMonth : DelayEvent
    {
        protected sealed override DateTime GetDelayStart(DateTime startedAt)
        {
            var result = new DateTime(startedAt.Year, startedAt.Month, 1);
            result.AddMonths(1);
            return result;
        }

        protected sealed override TimeSpan Delay => TimeSpan.Zero;
    }
}
