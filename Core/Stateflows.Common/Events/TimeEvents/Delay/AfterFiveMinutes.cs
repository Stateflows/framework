using System;

namespace Stateflows.Common
{
    /// <summary>Schedules an event five minutes after the next five-minute boundary.</summary>
    public sealed class AfterFiveMinutes : DelayEvent
    {
        /// <inheritdoc />
        protected override DateTime GetDelayStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, startedAt.Minute - (startedAt.Minute % 5), 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected override TimeSpan Delay => new TimeSpan(0, 5, 0);
    }
}
