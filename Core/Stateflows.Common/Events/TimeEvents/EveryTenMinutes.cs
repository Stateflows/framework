using System;

namespace Stateflows.Common
{
    public sealed class EveryTenMinutes : IntervalEvent
    {
        protected override TimeSpan Interval => new TimeSpan(0, 10, 0);
    }
}
