using Newtonsoft.Json;
using Stateflows.Common;
using System.Collections.Generic;

namespace Stateflows.Activities.Events
{
    public sealed class ExecutionRequest : Request<ExecutionResponse>
    {
        public Event InitializationEvent { get; set; } = new Initialize();

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<object> InputTokens { get; set; }
    }
}
