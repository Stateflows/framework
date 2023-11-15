using System;

namespace Stateflows.Common
{
    public sealed class AtEvery9AM : EveryOneDay
    {
        protected override DateTime TimeOfDay
            => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 9, 0, 0);
    }
}