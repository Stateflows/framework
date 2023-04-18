using System;

namespace Stateflows.Common
{
    public class TimeEvent : Event
    {
        public virtual bool ShouldTrigger(DateTime startedAt)
        {
            throw new NotImplementedException();
        }
    }
}
