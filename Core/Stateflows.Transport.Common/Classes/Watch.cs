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
        public List<Action<EventHolder>> Handlers { get; } = new List<Action<EventHolder>>();

        [JsonIgnore]
        public List<EventHolder> Notifications { get; }= new List<EventHolder>();
    }
}
