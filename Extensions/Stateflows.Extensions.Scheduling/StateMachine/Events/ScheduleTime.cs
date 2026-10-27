using System;

namespace Stateflows.Scheduler.StateMachine.Events
{
    /// <summary>Requests event delivery at one absolute date and time.</summary>
    public class ScheduleTime : ScheduleBase
    {
        /// <summary>Gets or sets the date and time at which the payloads are delivered.</summary>
        public DateTime Time { get; set; }
    }
}