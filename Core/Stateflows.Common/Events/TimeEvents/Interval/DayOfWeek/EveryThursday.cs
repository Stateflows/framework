using System;

namespace Stateflows.Common
{
    public class EveryThursday : EveryOneWeek
    {
        protected override sealed DayOfWeek Day => DayOfWeek.Thursday;
    }
}