using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring weekly event on Thursday at UTC midnight.</summary>
    public class EveryThursday : EveryOneWeek
    {
        /// <inheritdoc />
        protected sealed override DayOfWeek Day => DayOfWeek.Thursday;
    }
}