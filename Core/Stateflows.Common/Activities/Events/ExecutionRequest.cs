using Newtonsoft.Json;
using Stateflows.Common;
using System.Collections.Generic;

namespace Stateflows.Activities.Events
{
    public sealed class ExecutionRequest : Request<ExecutionResponse>
    {
        public ExecutionRequest(InitializationRequest initializationRequest, IEnumerable<Token> inputTokens)
        {
            InitializationRequest = initializationRequest;
            InputTokens = inputTokens;
        }

        public InitializationRequest InitializationRequest { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<Token> InputTokens { get; set; }
    }
}
