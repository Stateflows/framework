using System;
using Stateflows.Common.Utilities;

namespace Stateflows.Common
{
    /// <summary>Base class for events scheduled for a specific UTC trigger time.</summary>
    /// <remarks>Implementations derive trigger times from a supplied UTC start time; see the time-event patterns in the behaviors guide.</remarks>
    public class TimeEvent : SystemEvent
    {
        private DateTime triggerTime;

        /// <summary>Calculates the trigger time for a time event.</summary>
        /// <param name="startedAt">The normalized UTC start time.</param>
        /// <returns>The UTC instant at which the event should trigger.</returns>
        protected virtual DateTime GetTriggerTime(DateTime startedAt)
        {
            throw new NotImplementedException();
        }

        /// <summary>Calculates and assigns the trigger time from the supplied start instant.</summary>
        /// <param name="startedAt">UTC moment from which the trigger time is calculated; local values are normalized to UTC.</param>
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

        /// <summary>Gets or sets the signature of the consumer associated with this scheduled event.</summary>
        public string ConsumerSignature { get; set; }
    }
}
