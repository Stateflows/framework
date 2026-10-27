using Newtonsoft.Json;
using System.Collections.Generic;

namespace Stateflows.Common
{
    /// <summary>Ends a durable notification subscription to a behavior.</summary>
    [NoImplicitInitialization, NoTracing]
    public sealed class Unsubscribe
    {
        /// <summary>Gets or sets the identifier of the notification-producing behavior.</summary>
        public BehaviorId BehaviorId { get; set; }

        /// <summary>Gets or sets the names of the notification event types to unsubscribe from.</summary>
        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public List<string> NotificationNames { get; set; } = new List<string>();
    }
}
