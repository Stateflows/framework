using Stateflows.Common;

namespace Stateflows.Activities.Events
{
    public sealed class ExecutionRequest : Request<ExecutionResponse>
    {
        public ExecutionRequest(InitializationRequest initializationRequest)
        {
            InitializationRequest = initializationRequest;
        }

        public InitializationRequest InitializationRequest { get; }
    }
}
