using Stateflows.Common;
using Stateflows.Common.Classes;

namespace Stateflows.Transport.AspNetCore.WebApi.Responses
{
    internal class StateflowsInitializeResponse
    {
        public StateflowsInitializeResponse(RequestResult<InitializationResponse> result)
        {
            InitializationSuccessful = result.Response?.InitializationSuccessful ?? false;
        }

        public bool InitializationSuccessful { get; set; }
    }
}
