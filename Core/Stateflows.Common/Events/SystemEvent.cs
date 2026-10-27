using System;

namespace Stateflows.Common
{
    /// <summary>Base type for events generated internally by Stateflows.</summary>
    public class SystemEvent
    {
        /// <summary>Gets or sets the identifier of the event's processing context.</summary>
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
