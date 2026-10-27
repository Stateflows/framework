using System;

namespace Stateflows.Common
{
    /// <summary>Base class for a time event triggered after a delay from its calculated start instant.</summary>
    public abstract class DelayEvent : TimeEvent
    {
        /// <summary>Gets the delay added to the delay-start instant.</summary>
        protected abstract TimeSpan Delay { get; }

        /// <summary>Gets the instant from which <see cref="Delay"/> is applied.</summary>
        /// <param name="startedAt">The normalized UTC start time.</param>
        /// <returns>The UTC delay-start instant.</returns>
        protected virtual DateTime GetDelayStart(DateTime startedAt)
            => startedAt;

        /// <inheritdoc />
        protected override DateTime GetTriggerTime(DateTime startedAt)
            => GetDelayStart(startedAt).Add(Delay);
    }
}
