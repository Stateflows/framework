using System;

namespace Stateflows.Common
{
    public sealed class EveryOneWeek : IntervalEvent
    {
        protected override TimeSpan Interval => new TimeSpan(7, 0, 0, 0);
    }
}
