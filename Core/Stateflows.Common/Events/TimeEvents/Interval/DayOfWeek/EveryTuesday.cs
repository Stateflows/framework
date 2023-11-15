using System;

namespace Stateflows.Common
{
    public class EveryTuesday : EveryOneWeek
    {
        protected override sealed DayOfWeek Day => DayOfWeek.Tuesday;
    }
}