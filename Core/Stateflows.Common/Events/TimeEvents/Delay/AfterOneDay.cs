using System;

namespace Stateflows.Common
{
    /// <summary>Schedules an event one day after the start date's UTC midnight.</summary>
    public sealed class AfterOneDay : DelayEvent
    {
        /// <inheritdoc />
        protected override DateTime GetDelayStart(DateTime startedAt)
            => new DateTime(startedAt.Year, startedAt.Month, startedAt.Day, 0, 0, 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected override TimeSpan Delay => new TimeSpan(1, 0, 0, 0);
    }
}
