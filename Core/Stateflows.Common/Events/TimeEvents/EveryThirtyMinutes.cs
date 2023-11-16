using System;

namespace Stateflows.Common
{
    public sealed class EveryThirtyMinutes : IntervalEvent
    {
        protected override TimeSpan Interval => new TimeSpan(0, 30, 0);
    }
}
