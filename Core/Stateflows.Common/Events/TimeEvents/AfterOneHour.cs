using System;

namespace Stateflows.Common
{
    public sealed class AfterOneHour : DelayEvent
    {
        protected override TimeSpan Delay => new TimeSpan(1, 0, 0);
    }
}
