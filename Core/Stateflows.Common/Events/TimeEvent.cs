using System;

namespace Stateflows.Common
{
    public class TimeEvent : Event
    {
        public virtual bool ShouldTrigger(DateTime startedAt)
        {
            throw new NotImplementedException();
        }

        protected virtual DateTime GetTriggerTime(DateTime startedAt)
        {
            throw new NotImplementedException();
        }

        public void SetTriggerTime(DateTime startedAt)
            => TriggerTime = GetTriggerTime(startedAt);

        public DateTime TriggerTime { get; set; }

        public string EdgeIdentifier { get; set; }
    }
}
