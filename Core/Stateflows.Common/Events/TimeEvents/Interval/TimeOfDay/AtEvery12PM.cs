using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring daily event at 00:00 UTC as configured by its time-of-day value.</summary>
    public sealed class AtEvery12PM : EveryOneDay
    {
        /// <inheritdoc />
        protected override DateTime TimeOfDay
            => new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0, DateTimeKind.Utc);
    }
}