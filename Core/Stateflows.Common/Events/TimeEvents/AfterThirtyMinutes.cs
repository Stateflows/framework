using System;

namespace Stateflows.Common
{
    public sealed class AfterThirtyMinutes : DelayEvent
    {
        protected override TimeSpan Delay => new TimeSpan(0, 30, 0);
    }
}
