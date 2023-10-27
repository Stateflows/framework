using System;

namespace Stateflows.Common
{
    public sealed class AfterTenMinutes : DelayEvent
    {
        protected override TimeSpan Delay => new TimeSpan(0, 10, 0);
    }
}
