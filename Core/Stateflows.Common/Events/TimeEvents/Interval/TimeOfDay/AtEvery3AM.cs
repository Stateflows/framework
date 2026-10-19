using System;

namespace Stateflows.Common
{
    public sealed class AtEvery3AM : EveryOneDay
    {
        protected override DateTime TimeOfDay
            => new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 3, 0, 0, DateTimeKind.Utc);
    }
}