using System;

namespace Stateflows.Scheduler.StateMachine.Events
{
    /// <summary>Requests repeated event delivery at a fixed interval.</summary>
    public class ScheduleInterval : ScheduleBase
    {
        /// <summary>Gets or sets the time between consecutive deliveries.</summary>
        public TimeSpan Interval { get; set; }
    }
}