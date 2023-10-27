using System;

namespace Stateflows.Common
{
    public sealed class AfterOneMinute : DelayEvent
    {
        protected override TimeSpan Delay => new TimeSpan(0, 1, 0);
    }
}
