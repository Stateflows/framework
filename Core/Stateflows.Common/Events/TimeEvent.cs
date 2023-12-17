using System;

namespace Stateflows.Common
{
    public class TimeEvent : Event
    {
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
