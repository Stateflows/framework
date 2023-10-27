using System;

namespace Stateflows.Common
{
    public sealed class EveryOneDay : IntervalEvent
    {
        protected override TimeSpan Interval => new TimeSpan(1, 0, 0, 0);
    }
}
