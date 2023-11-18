using System;

namespace Stateflows.Common
{
    public sealed class AfterOneMinute : DelayEvent
    {
        protected override DateTime GetDelayStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, startedAt.Minute, 0);

        protected override TimeSpan Delay => new TimeSpan(0, 1, 0);
    }
}
