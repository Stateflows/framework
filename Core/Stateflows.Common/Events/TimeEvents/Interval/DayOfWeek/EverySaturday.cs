using System;

namespace Stateflows.Common
{
    public class EverySaturday : EveryOneWeek
    {
        protected override sealed DayOfWeek Day => DayOfWeek.Saturday;
    }
}