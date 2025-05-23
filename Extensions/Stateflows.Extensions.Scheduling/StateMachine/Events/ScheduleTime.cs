using System;

namespace Stateflows.Scheduler.StateMachine.Events
{
    public class ScheduleTime : ScheduleBase
    {
        public DateTime Time { get; set; }
    }
}