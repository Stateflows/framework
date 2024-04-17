using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stateflows.Common.Transport.Classes
{
    public class Watch
    {
        public string NotificationName { get; set; }

        public DateTime? LastNotificationCheck { get; set; }

        public int? MilisecondsSinceLastNotificationCheck { get; set; }

        [JsonIgnore]
        public List<Action<Notification>> Handlers { get; } = new List<Action<Notification>>();

        [JsonIgnore]
        public List<Notification> Notifications { get; }= new List<Notification>();
    }
}
