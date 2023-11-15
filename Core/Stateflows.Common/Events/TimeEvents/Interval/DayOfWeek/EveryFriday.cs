using System;

namespace Stateflows.Common
{
    public class EveryFriday : EveryOneWeek
    {
        protected override sealed DayOfWeek Day => DayOfWeek.Friday;
    }
}