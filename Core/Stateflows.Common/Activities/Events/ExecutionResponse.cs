using System.Collections.Generic;
using Newtonsoft.Json;
using Stateflows.Common;

namespace Stateflows.Activities.Events
{
    public sealed class ExecutionResponse : Response
    {
        public bool ExecutionSuccessful { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<object> OutputTokens { get; set; } = new List<object>();
    }
}
