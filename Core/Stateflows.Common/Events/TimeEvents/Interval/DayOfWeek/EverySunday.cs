using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring weekly event on Sunday at UTC midnight.</summary>
    public class EverySunday : EveryOneWeek
    {
        /// <inheritdoc />
        protected sealed override DayOfWeek Day => DayOfWeek.Sunday;
    }
}