using System;

namespace Stateflows.Common
{
    public class EverySunday : EveryOneWeek
    {
        protected sealed override DayOfWeek Day => DayOfWeek.Sunday;
    }
}