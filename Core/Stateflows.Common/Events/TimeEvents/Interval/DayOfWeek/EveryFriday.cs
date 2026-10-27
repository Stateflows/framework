using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring weekly event on Friday at UTC midnight.</summary>
    public class EveryFriday : EveryOneWeek
    {
        /// <inheritdoc />
        protected sealed override DayOfWeek Day => DayOfWeek.Friday;
    }
}