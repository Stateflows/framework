using System;
using NCrontab;
using Stateflows.Common;

namespace Stateflows.Events
{
    public abstract class CronEvent : RecurringEvent
    {
        internal abstract string CronExpression { get; }
        
        protected override DateTime GetTriggerTime(DateTime startedAt)
            => CrontabSchedule.Parse(CronExpression).GetNextOccurrence(startedAt);
    }
}