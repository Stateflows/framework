using System;

namespace Stateflows.Common
{
    public sealed class AfterFiveMinutes : DelayEvent
    {
        protected override TimeSpan Delay => new TimeSpan(0, 5, 0);
    }
}
