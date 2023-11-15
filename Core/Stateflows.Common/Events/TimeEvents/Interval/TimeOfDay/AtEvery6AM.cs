using System;

namespace Stateflows.Common
{
    public sealed class AtEvery6AM : EveryOneDay
    {
        protected override DateTime TimeOfDay
            => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 0, 0);
    }
}