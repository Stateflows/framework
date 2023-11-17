using System;

namespace Stateflows.Common
{
    public class EveryMonday : EveryOneWeek
    {
        protected override sealed DayOfWeek Day => DayOfWeek.Monday;
    }
}