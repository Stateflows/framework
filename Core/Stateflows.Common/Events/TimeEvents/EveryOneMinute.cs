using System;

namespace Stateflows.Common
{
    public sealed class EveryOneMinute : IntervalEvent
    {
        protected override TimeSpan Interval => new TimeSpan(0, 1, 0);
    }
}
