using System;

namespace Stateflows.Common
{
    public sealed class AtEvery12AM : EveryOneDay
    {
        protected override DateTime TimeOfDay
            => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
    }
}