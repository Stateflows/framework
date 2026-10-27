using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring weekly event on Monday at UTC midnight.</summary>
    public class EveryMonday : EveryOneWeek
    {
        /// <inheritdoc />
        protected sealed override DayOfWeek Day => DayOfWeek.Monday;
    }
}