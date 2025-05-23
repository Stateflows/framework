using System;
using System.Collections.Generic;
using System.Linq;
using Stateflows.Common;

namespace Stateflows.Scheduler.Classes
{
    public class Entry
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<BehaviorId> Recipients { get; set; } = null!;
        public List<EventHolder> Events { get; set; } = null!;
        public List<Rule> Rules { get; set; } = null!;

        public DateTime GetTriggerTime(DateTime lastCheck)
            => Rules
                .Select(r => r.GetTriggerTime(CreatedAt > lastCheck ? CreatedAt : lastCheck))
                .OrderBy(x => x)
                .First();
    }
}