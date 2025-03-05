using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stateflows.Common
{
    [NoImplicitInitialization]
    public sealed class NotificationsRequest : IRequest<NotificationsResponse>
    {
        public TimeSpan Period { get; set; }
        
        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public List<string> NotificationNames { get; set; } = new List<string>();
        
        public void Add<TNotification>()
            => NotificationNames.Add(Event<TNotification>.Name);
    }
}
