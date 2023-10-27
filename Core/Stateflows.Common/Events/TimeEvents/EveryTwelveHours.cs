using System;

namespace Stateflows.Common
{
    public sealed class EveryTwelveHours : IntervalEvent
    {
        protected override TimeSpan Interval => new TimeSpan(12, 0, 0);
    }
}
