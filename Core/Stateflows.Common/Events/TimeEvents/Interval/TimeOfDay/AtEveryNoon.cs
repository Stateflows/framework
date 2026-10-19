using System;

namespace Stateflows.Common
{
    public sealed class AtEveryNoon : EveryOneDay
    {
        protected override DateTime TimeOfDay
            => new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 12, 0, 0, DateTimeKind.Utc);
    }
}