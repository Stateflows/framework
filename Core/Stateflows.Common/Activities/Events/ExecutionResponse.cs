using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stateflows.Activities.Events
{
    public sealed class ExecutionResponse
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<object> OutputTokens { get; set; } = new List<object>();
    }
}
