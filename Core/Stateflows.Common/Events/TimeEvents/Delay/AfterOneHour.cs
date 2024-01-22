using System;

namespace Stateflows.Common
{
    public class AfterOneHour : DelayEvent
    {
        protected sealed override DateTime GetDelayStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, 0, 0);

        protected virtual int Hours => 1;

        protected sealed override TimeSpan Delay => new TimeSpan(Hours, 0, 0);
    }
}
