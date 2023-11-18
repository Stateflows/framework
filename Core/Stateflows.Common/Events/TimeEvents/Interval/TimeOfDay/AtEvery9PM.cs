using System;

namespace Stateflows.Common
{
    public sealed class AtEvery9PM : EveryOneDay
    {
        protected override DateTime TimeOfDay
            => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 21, 0, 0);
    }
}