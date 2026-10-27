using System;

namespace Stateflows.Common
{
    /// <summary>Schedules an event one minute after the start-time minute boundary.</summary>
    public sealed class AfterOneMinute : DelayEvent
    {
        /// <inheritdoc />
        protected override DateTime GetDelayStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, startedAt.Minute, 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected override TimeSpan Delay => new TimeSpan(0, 1, 0);
    }
}
