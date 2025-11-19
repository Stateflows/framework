using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Stateflows.Common
{
    [Retain, StrictOwnership]
    public class BehaviorInfo
    {
        public BehaviorId Id { get; set; }
        
        public BehaviorStatus BehaviorStatus { get; set; }
        
        public string BehaviorStatusText => Enum.GetName(typeof(BehaviorStatus), BehaviorStatus);

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<string> ExpectedEvents { get; set; }

        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
