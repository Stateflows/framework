using System;

namespace Stateflows.Common
{
    public sealed class AfterOneHour : DelayEvent
    {
        protected override DateTime GetDelayStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, 0, 0);

        protected override TimeSpan Delay => new TimeSpan(1, 0, 0);
    }
}
