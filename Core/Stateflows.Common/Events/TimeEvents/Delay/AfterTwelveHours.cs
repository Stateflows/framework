using System;

namespace Stateflows.Common
{
    public sealed class AfterTwelveHours : DelayEvent
    {
        protected override DateTime GetDelayStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour - (startedAt.Hour % 12), 0, 0);

        protected override TimeSpan Delay => new TimeSpan(12, 0, 0);
    }
}
