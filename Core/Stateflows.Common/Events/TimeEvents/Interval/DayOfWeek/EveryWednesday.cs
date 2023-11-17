using System;

namespace Stateflows.Common
{
    public class EveryWednesday : EveryOneWeek
    {
        protected override sealed DayOfWeek Day => DayOfWeek.Wednesday;
    }
}