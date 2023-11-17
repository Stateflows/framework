using System;

namespace Stateflows.Common
{
    public sealed class AfterTenMinutes : DelayEvent
    {
        protected override DateTime GetDelayStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, startedAt.Minute - (startedAt.Minute % 10), 0);

        protected override TimeSpan Delay => new TimeSpan(0, 10, 0);
    }
}
