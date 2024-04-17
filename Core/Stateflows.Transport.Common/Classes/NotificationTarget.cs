using Newtonsoft.Json;
using System.Collections.Generic;

namespace Stateflows.Common.Transport.Classes
{
    public class NotificationTarget
    {
        public BehaviorId Id { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<Watch> Watches { get; set; }
    }
}
