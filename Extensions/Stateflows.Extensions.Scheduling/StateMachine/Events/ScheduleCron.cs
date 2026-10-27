using System.Collections.Generic;

namespace Stateflows.Scheduler.StateMachine.Events
{
    /// <summary>Requests repeated event delivery at the occurrences described by cron expressions.</summary>
    public class ScheduleCron : ScheduleBase
    {
        /// <summary>Gets or sets the cron expressions used to determine delivery times.</summary>
        public List<string> CronExpressions { get; set; } = new List<string>();
    }
}