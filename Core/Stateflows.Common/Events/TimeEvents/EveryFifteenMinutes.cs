using System;

namespace Stateflows.Common
{
    public sealed class EveryFifteenMinutes : IntervalEvent
    {
        protected override TimeSpan Interval => new TimeSpan(0, 15, 0);
    }
}
