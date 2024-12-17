using System;

namespace Stateflows.Common
{
    public abstract class DelayEvent : TimeEvent
    {
        protected abstract TimeSpan Delay { get; }

        protected virtual DateTime GetDelayStart(DateTime startedAt)
            => startedAt;

        protected override DateTime GetTriggerTime(DateTime startedAt)
            => GetDelayStart(startedAt).Add(Delay).Add(Delay);
    }
}
