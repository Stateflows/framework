using System;

namespace Stateflows.Common
{
    /// <summary>Schedules the event at the first day of the start-time month, according to the current implementation.</summary>
    public sealed class AfterOneMonth : DelayEvent
    {
        /// <inheritdoc />
        protected sealed override DateTime GetDelayStart(DateTime startedAt)
        {
            var result = new DateTime(startedAt.Year, startedAt.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            result.AddMonths(1);
            return result;
        }

        /// <inheritdoc />
        protected sealed override TimeSpan Delay => TimeSpan.Zero;
    }
}
