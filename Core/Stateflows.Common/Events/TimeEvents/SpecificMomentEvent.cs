using System;

namespace Stateflows.Common
{
    public abstract class SpecificMomentEvent : TimeEvent
    {
        protected abstract DateTime Moment { get; }

        protected override DateTime GetTriggerTime(DateTime startedAt)
            => Moment;
    }
}
