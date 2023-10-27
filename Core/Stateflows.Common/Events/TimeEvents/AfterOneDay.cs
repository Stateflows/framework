using System;

namespace Stateflows.Common
{
    public sealed class AfterOneDay : DelayEvent
    {
        protected override TimeSpan Delay => new TimeSpan(1, 0, 0, 0);
    }
}
