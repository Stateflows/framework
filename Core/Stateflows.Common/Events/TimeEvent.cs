using System;
using Stateflows.Common.Utilities;

namespace Stateflows.Common
{
    public class TimeEvent : SystemEvent
    {
        private DateTime triggerTime;

        protected virtual DateTime GetTriggerTime(DateTime startedAt)
        {
            throw new NotImplementedException();
        }

        /// <param name="startedAt">UTC moment from which the trigger time is calculated.</param>
        public void SetTriggerTime(DateTime startedAt)
            => TriggerTime = GetTriggerTime(UtcTime.Normalize(startedAt));

        /// <summary>
        /// UTC trigger time. Local values are converted to UTC; unspecified values are treated as UTC.
        /// </summary>
        public DateTime TriggerTime
        {
            get => triggerTime;
            set => triggerTime = UtcTime.Normalize(value);
        }

        public string ConsumerSignature { get; set; }
    }
}
