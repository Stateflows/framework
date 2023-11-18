using System;

namespace Stateflows.Common
{
    public sealed class AtEvery3AM : EveryOneDay
    {
        protected override DateTime TimeOfDay
            => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 3, 0, 0);
    }
}