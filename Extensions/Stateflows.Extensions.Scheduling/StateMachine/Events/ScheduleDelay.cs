using System;

namespace Stateflows.Scheduler.StateMachine.Events
{
    /// <summary>Requests event delivery once after a delay.</summary>
    public class ScheduleDelay : ScheduleBase
    {
        /// <summary>Gets or sets the delay from schedule creation until delivery.</summary>
        public TimeSpan Delay { get; set; }
    }
}