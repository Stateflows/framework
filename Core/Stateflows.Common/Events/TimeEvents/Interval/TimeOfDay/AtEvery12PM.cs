using System;

namespace Stateflows.Common
{
    public sealed class AtEvery12PM : EveryOneDay
    {
        protected override DateTime TimeOfDay
            => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
    }
}