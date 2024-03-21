using Newtonsoft.Json;
using System.Collections.Generic;

namespace Stateflows.Common
{
    public class BehaviorStatusResponse : Response
    {
        public override string Name => nameof(BehaviorStatusResponse);

        public BehaviorStatus BehaviorStatus { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<string> ExpectedEvents { get; set; }
    }
}
