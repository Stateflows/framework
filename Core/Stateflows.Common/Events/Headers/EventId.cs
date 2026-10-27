using System;
using System.Text.Json.Serialization;

namespace Stateflows.Common
{
    /// <summary>
    /// Supplies a stable identifier when constructing an incoming event envelope.
    /// </summary>
    public sealed class EventId : EventHeader
    {
        /// <summary>Creates an event-id header whose identifier can be assigned later.</summary>
        [Newtonsoft.Json.JsonConstructor, JsonConstructor]
        public EventId()
        { }

        /// <summary>Creates an event-id header with a specified delivery identifier.</summary>
        /// <param name="id">The stable identifier to use for event deduplication.</param>
        public EventId(Guid id)
        {
            Id = id;
        }

        /// <summary>Gets or sets the stable event delivery identifier.</summary>
        public Guid Id { get; set; }
    }
}
