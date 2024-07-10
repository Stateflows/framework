using Newtonsoft.Json;
using Stateflows.Common;
using System.Collections.Generic;

namespace Stateflows.Activities.Events
{
    public sealed class ExecutionRequest : Request<ExecutionResponse>
    {
        //public ExecutionRequest(IEnumerable<object> inputTokens)
        //{
        //    InputTokens = inputTokens;
        //}

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<object> InputTokens { get; set; }
    }
}
