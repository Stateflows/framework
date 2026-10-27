using System;

namespace Stateflows.Common
{
    /// <summary>Schedules a recurring weekly event on Saturday at UTC midnight.</summary>
    public class EverySaturday : EveryOneWeek
    {
        /// <inheritdoc />
        protected sealed override DayOfWeek Day => DayOfWeek.Saturday;
    }
}