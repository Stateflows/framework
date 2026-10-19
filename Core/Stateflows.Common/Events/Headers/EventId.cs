using System;
using System.Text.Json.Serialization;

namespace Stateflows.Common
{
    /// <summary>
    /// Supplies a stable identifier when constructing an incoming event envelope.
    /// </summary>
    public sealed class EventId : EventHeader
    {
        [Newtonsoft.Json.JsonConstructor, JsonConstructor]
        public EventId()
        { }

        public EventId(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}
