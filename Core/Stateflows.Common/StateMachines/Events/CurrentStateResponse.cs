using Newtonsoft.Json;
using Stateflows.Common;
using System.Collections.Generic;

namespace Stateflows.StateMachines.Events
{
    public sealed class CurrentStateResponse : BehaviorStatus
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<string> StatesStack { get; set; }
    }
}
