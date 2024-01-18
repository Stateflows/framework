using Stateflows.Common;

namespace Stateflows.Transport.AspNetCore.WebApi.Responses
{
    internal class StateflowsInitializeResponse
    {
        public StateflowsInitializeResponse(RequestResult<InitializationResponse> result)
        {
            InitializationSuccessful = result.Response?.InitializationSuccessful ?? false;
            Status = result.Status;
        }

        public bool InitializationSuccessful { get; set; }

        public EventStatus Status { get; set; }
    }
}
