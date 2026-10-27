using System;
using NCrontab;
using Stateflows.Common;

namespace Stateflows.Events
{
    /// <summary>Base class for recurring events whose next occurrence is determined by a cron expression.</summary>
    public abstract class CronEvent : RecurringEvent
    {
        internal abstract string CronExpression { get; }
        
        /// <inheritdoc/>
        protected override DateTime GetTriggerTime(DateTime startedAt)
            => CrontabSchedule.Parse(CronExpression).GetNextOccurrence(startedAt);
    }
}