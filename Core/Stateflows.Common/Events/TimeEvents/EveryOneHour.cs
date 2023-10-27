using System;

namespace Stateflows.Common
{
    public sealed class EveryOneHour : IntervalEvent
    {
        protected override TimeSpan Interval => new TimeSpan(1, 0, 0);
    }
}
