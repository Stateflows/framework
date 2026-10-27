using System;
using Newtonsoft.Json;

namespace Stateflows.Common.Classes
{
    /// <summary>Describes a time-triggered event scheduled for a behavior.</summary>
    public class TimeToken : EventToken
    {
        /// <summary>Gets or sets the identifier used to distinguish this timer.</summary>
        public string Id { get; set; }

        /// <summary>Gets or sets the time when the token was created.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Gets or sets the time event associated with this token.</summary>
        public new TimeEvent Event { get; set; }

        /// <summary>Gets or sets the identifier of the transition edge associated with the timer, when applicable.</summary>
        public string EdgeIdentifier { get; set; }
    }
}
