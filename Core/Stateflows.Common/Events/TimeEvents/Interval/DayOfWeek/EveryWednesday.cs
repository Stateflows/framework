using System;

namespace Stateflows.Common
{
    public class EveryWednesday : EveryOneWeek
    {
        protected sealed override DayOfWeek Day => DayOfWeek.Wednesday;
    }
}