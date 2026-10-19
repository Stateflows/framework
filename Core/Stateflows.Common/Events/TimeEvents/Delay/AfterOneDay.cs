using System;

namespace Stateflows.Common
{
    public sealed class AfterOneDay : DelayEvent
    {
        protected override DateTime GetDelayStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, 0, 0, 0, DateTimeKind.Utc);

        protected override TimeSpan Delay => new TimeSpan(1, 0, 0, 0);
    }
}
