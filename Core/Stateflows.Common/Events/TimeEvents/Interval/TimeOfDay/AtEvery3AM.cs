using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring daily event at 03:00 UTC.</summary>
    public sealed class AtEvery3AM : EveryOneDay
    {
        /// <inheritdoc />
        protected override DateTime TimeOfDay
            => new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 3, 0, 0, DateTimeKind.Utc);
    }
}