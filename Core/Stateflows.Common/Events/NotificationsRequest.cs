using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stateflows.Common
{
    /// <summary>Requests stored notifications by event name and time window.</summary>
    [NoImplicitInitialization, NoTracing, NoDeduplication]
    public sealed class NotificationsRequest : IRequest<NotificationsResponse>
    {
        /// <summary>Gets or sets the period for which notifications are requested.</summary>
        public TimeSpan Period { get; set; }
        
        /// <summary>Gets or sets the requested notification names.</summary>
        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public List<string> NotificationNames { get; set; } = new List<string>();
        
        /// <summary>Adds a notification type by its Stateflows event name.</summary>
        /// <typeparam name="TNotification">The notification payload type.</typeparam>
        public void Add<TNotification>()
            => NotificationNames.Add(Event<TNotification>.Name);
    }
}
