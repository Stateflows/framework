using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring weekly event on Tuesday at UTC midnight.</summary>
    public class EveryTuesday : EveryOneWeek
    {
        /// <inheritdoc />
        protected sealed override DayOfWeek Day => DayOfWeek.Tuesday;
    }
}