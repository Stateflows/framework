using System;

namespace Stateflows.Common
{
    public abstract class DelayEvent : TimeEvent
    {
        protected abstract TimeSpan Delay { get; }

        public override sealed bool ShouldTrigger(DateTime startedAt)
            => DateTime.Now >= startedAt.Add(Delay);
    }
}
