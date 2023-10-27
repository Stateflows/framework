using System;

namespace Stateflows.Common
{
    public sealed class AfterTwelveHours : DelayEvent
    {
        protected override TimeSpan Delay => new TimeSpan(12, 0, 0);
    }
}
