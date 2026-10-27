using System;

namespace Stateflows.Scheduler.StateMachine.Events
{
    /// <summary>Response returned when the scheduler accepts a schedule request.</summary>
    public class ScheduleResponse
    {
        /// <summary>Gets or sets the accepted schedule's identifier.</summary>
        public Guid Id { get; set; }
    }
}