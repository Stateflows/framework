using System.Collections.Generic;

namespace Stateflows.Common
{
    /// <summary>Contains the stored notifications returned by a notification query.</summary>
    public sealed class NotificationsResponse
    {
        /// <summary>Gets or sets the matching event envelopes.</summary>
        public IEnumerable<EventHolder> Notifications { get; set; }
    }
}
