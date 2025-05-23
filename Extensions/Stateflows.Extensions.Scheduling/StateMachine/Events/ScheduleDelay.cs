using System;

namespace Stateflows.Scheduler.StateMachine.Events
{
    public class ScheduleDelay : ScheduleBase
    {
        public TimeSpan Delay { get; set; }
    }
}