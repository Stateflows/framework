using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Scheduler.StateMachine.Events
{
    /// <summary>Base request describing event payloads and behavior recipients for one or more schedules.</summary>
    public abstract class ScheduleBase : IRequest<ScheduleResponse>
    {
        /// <summary>Gets or sets the behavior instances that should receive the scheduled payloads.</summary>
        public List<BehaviorId> Recipients { get; set; } = new List<BehaviorId>();
        /// <summary>Gets or sets the event payloads to deliver when a schedule triggers.</summary>
        public List<EventHolder> Events { get; set; } = new List<EventHolder>();
    }
}