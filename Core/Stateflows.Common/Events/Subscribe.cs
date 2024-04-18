using Newtonsoft.Json;
using System.Collections.Generic;

namespace Stateflows.Common
{
    public sealed class Subscribe : Command
    {
        public BehaviorId BehaviorId { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public List<string> NotificationNames { get; set; } = new List<string>();
    }
}
