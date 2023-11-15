using System;

namespace Stateflows.Common
{
    public sealed class AtEveryNoon : EveryOneDay
    {
        protected override DateTime TimeOfDay
            => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
    }
}