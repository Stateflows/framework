using System;

namespace Stateflows.Common
{
    /// <summary>Base class for a recurring event scheduled at a fixed interval.</summary>
    public abstract class IntervalEvent : RecurringEvent
    {
        /// <summary>Gets the duration between recurring trigger instants.</summary>
        protected abstract TimeSpan Interval { get; }

        /// <summary>Gets the instant from which the interval schedule is aligned.</summary>
        /// <param name="startedAt">The normalized UTC start time.</param>
        /// <returns>The UTC instant from which <see cref="Interval"/> is applied.</returns>
        protected virtual DateTime GetIntervalStart(DateTime startedAt)
            => startedAt;

        /// <inheritdoc />
        protected override DateTime GetTriggerTime(DateTime startedAt)
            => GetIntervalStart(startedAt).Add(Interval);
    }
}
