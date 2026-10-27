using System;

namespace Stateflows.Common
{
    /// <summary>Schedules an event fifteen minutes after the next fifteen-minute boundary.</summary>
    public sealed class AfterFifteenMinutes : DelayEvent
    {
        /// <inheritdoc />
        protected override DateTime GetDelayStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, startedAt.Minute - (startedAt.Minute % 15), 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected override TimeSpan Delay => new TimeSpan(0, 15, 0);
    }
}
