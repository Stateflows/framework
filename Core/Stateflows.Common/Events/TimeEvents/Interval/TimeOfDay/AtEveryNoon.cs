using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring daily event at 12:00 UTC.</summary>
    public sealed class AtEveryNoon : EveryOneDay
    {
        /// <inheritdoc />
        protected override DateTime TimeOfDay
            => new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 12, 0, 0, DateTimeKind.Utc);
    }
}