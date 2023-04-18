using Stateflows.Common;

namespace Examples.Common
{
    public class OneMinuteEvent : DelayEvent
    {
        protected override TimeSpan Delay => new TimeSpan(0, 1, 0);
    }
}