using System;

namespace Stateflows.Common
{
    public class EverySaturday : EveryOneWeek
    {
        protected sealed override DayOfWeek Day => DayOfWeek.Saturday;
    }
}