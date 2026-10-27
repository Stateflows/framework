using System;

namespace Stateflows.Common
{
    /// <summary>Schedules an event thirty minutes after the next half-hour boundary.</summary>
    public sealed class AfterThirtyMinutes : DelayEvent
    {
        /// <inheritdoc />
        protected override DateTime GetDelayStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, startedAt.Minute - (startedAt.Minute % 30), 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected override TimeSpan Delay => new TimeSpan(0, 30, 0);
    }
}
