using System;
using System.Collections.Generic;

namespace Stateflows.Scheduler.StateMachine.Events
{
    public class Unschedule
    {
        public List<Guid> Ids { get; set; } = null!;
    }
}