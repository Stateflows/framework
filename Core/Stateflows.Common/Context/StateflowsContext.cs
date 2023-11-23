using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stateflows.Common.Context
{
    public class StateflowsContext
    {
        [JsonIgnore]
        public BehaviorId Id { get; set; }

        public int Version { get; set; } = 0;

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

        public Dictionary<string, string> GlobalValues { get; } = new Dictionary<string, string>();
    }
}
