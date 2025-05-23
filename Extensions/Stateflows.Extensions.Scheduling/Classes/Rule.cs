using System;
using System.Collections.Generic;
using System.Linq;
using NCrontab;

namespace Stateflows.Scheduler.Classes
{
    public enum RuleKind
    {
        Time,
        Delay,
        Interval,
        Cron
    }
    
    public class Rule
    {
        public RuleKind Kind { get; set; }
        public DateTime Time { get; set; }
        public TimeSpan Delay { get; set; }
        public TimeSpan Interval { get; set; }
        public List<string> CronExpressions { get; set; } = null!;

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