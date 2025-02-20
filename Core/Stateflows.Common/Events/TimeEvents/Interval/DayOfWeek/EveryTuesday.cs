using System;

namespace Stateflows.Common
{
    public class EveryTuesday : EveryOneWeek
    {
        protected sealed override DayOfWeek Day => DayOfWeek.Tuesday;
    }
}