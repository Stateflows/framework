using System.Collections.Generic;

namespace Stateflows.Scheduler.StateMachine.Events
{
    public class ScheduleCron : ScheduleBase
    {
        public List<string> CronExpressions { get; set; } = new List<string>();
    }
}