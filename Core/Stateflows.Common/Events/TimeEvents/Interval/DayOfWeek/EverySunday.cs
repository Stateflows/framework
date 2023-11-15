using System;

namespace Stateflows.Common
{
    public class EverySunday : EveryOneWeek
    {
        protected override sealed DayOfWeek Day => DayOfWeek.Sunday;
    }
}