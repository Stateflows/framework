using System;

namespace Stateflows.Common
{
    public class EveryMonday : EveryOneWeek
    {
        protected sealed override DayOfWeek Day => DayOfWeek.Monday;
    }
}