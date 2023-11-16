using System;

namespace Stateflows.Common
{
    public sealed class EveryFiveMinutes : IntervalEvent
    {
        protected override TimeSpan Interval => new TimeSpan(0, 5, 0);
    }
}
