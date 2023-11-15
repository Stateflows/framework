using System;

namespace Stateflows.Common
{
    public sealed class AtEvery3PM : EveryOneDay
    {
        protected override DateTime TimeOfDay
            => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 15, 0, 0);
    }
}