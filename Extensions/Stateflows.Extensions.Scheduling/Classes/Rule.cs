using System;
using System.Collections.Generic;
using System.Linq;
using NCrontab;

namespace Stateflows.Scheduler.Classes
{
    /// <summary>Identifies the schedule rule used to calculate a delivery time.</summary>
    public enum RuleKind
    {
        /// <summary>A schedule for one fixed date and time.</summary>
        Time,
        /// <summary>A schedule relative to its creation time.</summary>
        Delay,
        /// <summary>A recurring schedule with a fixed interval.</summary>
        Interval,
        /// <summary>A recurring schedule defined by one or more cron expressions.</summary>
        Cron
    }
    
    /// <summary>Stores one time, delay, interval, or cron rule for a scheduled event.</summary>
    public class Rule
    {
        /// <summary>Gets or sets which rule data is used to calculate the next trigger time.</summary>
        public RuleKind Kind { get; set; }
        /// <summary>Gets or sets the absolute trigger time used by a <see cref="RuleKind.Time"/> rule.</summary>
        public DateTime Time { get; set; }
        /// <summary>Gets or sets the creation-relative delay used by a <see cref="RuleKind.Delay"/> rule.</summary>
        public TimeSpan Delay { get; set; }
        /// <summary>Gets or sets the repetition interval used by a <see cref="RuleKind.Interval"/> rule.</summary>
        public TimeSpan Interval { get; set; }
        /// <summary>Gets or sets cron expressions used by a <see cref="RuleKind.Cron"/> rule.</summary>
        public List<string> CronExpressions { get; set; } = null!;

        /// <summary>Calculates the trigger time specified by this rule after the supplied start time.</summary>
        /// <param name="startedAt">The point from which relative or recurring rules are evaluated.</param>
        /// <returns>The trigger date and time produced by the selected rule.</returns>
        public DateTime GetTriggerTime(DateTime startedAt)
            => Kind switch
            {
                RuleKind.Delay => startedAt + Delay,
                RuleKind.Interval => startedAt + Interval,
                RuleKind.Time => Time,
                RuleKind.Cron => CronExpressions
                    .Select(CrontabSchedule.Parse)
                    .Select(s => s.GetNextOccurrence(startedAt))
                    .OrderBy(s => s.Date)
                    .First(),
                _ => throw new NotSupportedException($"{Kind} is not supported.")
            };
    }
}