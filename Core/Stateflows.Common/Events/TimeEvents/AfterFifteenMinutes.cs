using System;

namespace Stateflows.Common
{
    public sealed class AfterFifteenMinutes : DelayEvent
    {
        protected override TimeSpan Delay => new TimeSpan(0, 15, 0);
    }
}
