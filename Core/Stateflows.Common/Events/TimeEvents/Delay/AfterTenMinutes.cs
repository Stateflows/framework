using System;

namespace Stateflows.Common
{
    /// <summary>Schedules an event ten minutes after the next ten-minute boundary.</summary>
    public sealed class AfterTenMinutes : DelayEvent
    {
        /// <inheritdoc />
        protected override DateTime GetDelayStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, startedAt.Hour, startedAt.Minute - (startedAt.Minute % 10), 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected override TimeSpan Delay => new TimeSpan(0, 10, 0);
    }
}
