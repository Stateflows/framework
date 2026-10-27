using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring daily event at 18:00 UTC.</summary>
    public sealed class AtEvery6PM : EveryOneDay
    {
        /// <inheritdoc />
        protected override DateTime TimeOfDay
            => new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 18, 0, 0, DateTimeKind.Utc);
    }
}