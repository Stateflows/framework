using System;

namespace Stateflows.Common
{
    /// <summary>Base class for an event scheduled at a configured absolute instant.</summary>
    public abstract class SpecificMomentEvent : TimeEvent
    {
        /// <summary>Gets the absolute UTC instant at which the event should trigger.</summary>
        protected abstract DateTime Moment { get; }

        /// <inheritdoc />
        protected override DateTime GetTriggerTime(DateTime startedAt)
            => Moment;
    }
}
