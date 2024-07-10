using System;

namespace Stateflows.Common
{
    public class TimeEvent : SystemEvent
    {
        protected virtual DateTime GetTriggerTime(DateTime startedAt)
        {
            throw new NotImplementedException();
        }

        public void SetTriggerTime(DateTime startedAt)
            => TriggerTime = GetTriggerTime(startedAt);

        public DateTime TriggerTime { get; set; }

        public string ConsumerSignature { get; set; }
    }
}
