using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring daily event at 09:00 UTC.</summary>
    public sealed class AtEvery9AM : EveryOneDay
    {
        /// <inheritdoc />
        protected override DateTime TimeOfDay
            => new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 9, 0, 0, DateTimeKind.Utc);
    }
}