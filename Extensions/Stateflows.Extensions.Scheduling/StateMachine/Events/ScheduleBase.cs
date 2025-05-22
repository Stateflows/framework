using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Scheduler.StateMachine.Events
{
    public abstract class ScheduleBase : IRequest<ScheduleResponse>
    {
        public List<BehaviorId> Recipients { get; set; } = new List<BehaviorId>();
        public List<EventHolder> Events { get; set; } = new List<EventHolder>();
    }
}