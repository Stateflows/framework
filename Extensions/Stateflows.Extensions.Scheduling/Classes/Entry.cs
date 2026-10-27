using System;
using System.Collections.Generic;
using System.Linq;
using Stateflows.Common;

namespace Stateflows.Scheduler.Classes
{
    /// <summary>Represents a persisted scheduled delivery, including its recipients, payloads, and trigger rules.</summary>
    public class Entry
    {
        /// <summary>Gets or sets the identifier returned to the caller when the schedule is accepted.</summary>
        public Guid Id { get; set; }
        /// <summary>Gets or sets when the schedule was created.</summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>Gets or sets the behavior instances that receive the scheduled event payloads.</summary>
        public List<BehaviorId> Recipients { get; set; } = null!;
        /// <summary>Gets or sets the event payloads delivered to each recipient when a rule triggers.</summary>
        public List<EventHolder> Events { get; set; } = null!;
        /// <summary>Gets or sets the rules that determine when this entry is due.</summary>
        public List<Rule> Rules { get; set; } = null!;

        /// <summary>Returns the earliest trigger time from this entry's rules after the last scheduler check.</summary>
        /// <param name="lastCheck">The time of the previous schedule check.</param>
        /// <returns>The earliest next trigger time among the entry's rules.</returns>
        public DateTime GetTriggerTime(DateTime lastCheck)
            => Rules
                .Select(r => r.GetTriggerTime(CreatedAt > lastCheck ? CreatedAt : lastCheck))
                .OrderBy(x => x)
                .First();
    }
}