using System;

namespace Stateflows.Common
{
    public sealed class AtEvery6PM : EveryOneDay
    {
        protected override DateTime TimeOfDay
            => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 18, 0, 0);
    }
}