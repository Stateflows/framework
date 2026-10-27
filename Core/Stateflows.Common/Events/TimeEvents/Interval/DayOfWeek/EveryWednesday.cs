using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring weekly event on Wednesday at UTC midnight.</summary>
    public class EveryWednesday : EveryOneWeek
    {
        /// <inheritdoc />
        protected sealed override DayOfWeek Day => DayOfWeek.Wednesday;
    }
}