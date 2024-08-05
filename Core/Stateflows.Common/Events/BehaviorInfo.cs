using Newtonsoft.Json;
using System.Collections.Generic;

namespace Stateflows.Common
{
    public class BehaviorInfo
    {
        //public override string Name => nameof(BehaviorInfo);

        public BehaviorStatus BehaviorStatus { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<string> ExpectedEvents { get; set; }
    }
}
