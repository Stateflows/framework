using System;

namespace Stateflows.Common
{
    /// <summary>Schedules an event after the next whole-hour boundary.</summary>
    public class AfterOneHour : DelayEvent
    {
        /// <inheritdoc />
        protected sealed override DateTime GetDelayStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, 0, 0, DateTimeKind.Utc);

        /// <summary>Gets the number of hours to wait; subclasses may override the default one-hour delay.</summary>
        protected virtual int Hours => 1;

        /// <inheritdoc />
        protected sealed override TimeSpan Delay => new TimeSpan(Hours, 0, 0);
    }
}
