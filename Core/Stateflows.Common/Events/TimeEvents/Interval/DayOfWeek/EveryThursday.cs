using System;

namespace Stateflows.Common
{
    public class EveryThursday : EveryOneWeek
    {
        protected sealed override DayOfWeek Day => DayOfWeek.Thursday;
    }
}