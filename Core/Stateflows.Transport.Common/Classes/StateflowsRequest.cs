using Newtonsoft.Json;
using System.Collections.Generic;

namespace Stateflows.Common.Transport.Classes
{
    public class StateflowsRequest
    {
        public object Event { get; set; }

        public BehaviorId BehaviorId { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<Watch> Watches { get; set; }
    }
}
