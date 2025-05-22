using System;

namespace Stateflows.Scheduler.StateMachine.Events
{
    public class ScheduleInterval : ScheduleBase
    {
        public TimeSpan Interval { get; set; }
    }
}