using Newtonsoft.Json;
using System.Collections.Generic;

namespace Stateflows.Common
{
    /// <summary>Creates a durable subscription to notifications from a behavior.</summary>
    [NoImplicitInitialization, NoTracing]
    public sealed class Subscribe
    {
        /// <summary>Gets or sets the identifier of the notification-producing behavior.</summary>
        public BehaviorId BehaviorId { get; set; }

        /// <summary>Gets or sets the names of the notification event types to subscribe to.</summary>
        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public List<string> NotificationNames { get; set; } = [];
    }
}
