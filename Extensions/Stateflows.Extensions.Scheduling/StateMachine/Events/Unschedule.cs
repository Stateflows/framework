using System;
using System.Collections.Generic;

namespace Stateflows.Scheduler.StateMachine.Events
{
    /// <summary>Request to remove one or more schedules by identifier.</summary>
    public class Unschedule
    {
        /// <summary>Gets or sets the identifiers of schedules to remove.</summary>
        public List<Guid> Ids { get; set; } = null!;
    }
}