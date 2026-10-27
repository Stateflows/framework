using System;

namespace Stateflows.Common
{
    /// <summary>Schedules an event twelve hours after the next twelve-hour boundary.</summary>
    public sealed class AfterTwelveHours : DelayEvent
    {
        /// <inheritdoc />
        protected override DateTime GetDelayStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour - (startedAt.Hour % 12), 0, 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected override TimeSpan Delay => new TimeSpan(12, 0, 0);
    }
}
