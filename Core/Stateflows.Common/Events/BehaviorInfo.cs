using Newtonsoft.Json;
using System.Collections.Generic;

namespace Stateflows.Common
{
    public class BehaviorInfo
    {
        public BehaviorStatus BehaviorStatus { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<string> ExpectedEvents { get; set; }
    }
}
