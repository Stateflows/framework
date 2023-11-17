using System;

namespace Stateflows.Common
{
    public sealed class AfterFifteenMinutes : DelayEvent
    {
        protected override DateTime GetDelayStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, startedAt.Minute - (startedAt.Minute % 15), 0);

        protected override TimeSpan Delay => new TimeSpan(0, 15, 0);
    }
}
