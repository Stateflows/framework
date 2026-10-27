using System;

namespace Stateflows.Common
{
    /// <summary>Assigns a stable event name to a CLR event type.</summary>
    /// <remarks>When omitted, Stateflows derives the event name from the type.</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class EventAttribute : Attribute
    {
        /// <summary>Gets or sets the event name.</summary>
        public string Name { get; set; }

        /// <summary>Creates an event-name attribute.</summary>
        /// <param name="name">The stable event name.</param>
        public EventAttribute(string name)
        {
            Name = name;
        }
    }
}
