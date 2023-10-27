using System;

namespace Stateflows.Common
{
    public sealed class AfterOneWeek : DelayEvent
    {
        protected override TimeSpan Delay => new TimeSpan(7, 0, 0, 0);
    }
}
