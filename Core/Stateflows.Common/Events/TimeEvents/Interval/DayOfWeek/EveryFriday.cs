using System;

namespace Stateflows.Common
{
    public class EveryFriday : EveryOneWeek
    {
        protected sealed override DayOfWeek Day => DayOfWeek.Friday;
    }
}