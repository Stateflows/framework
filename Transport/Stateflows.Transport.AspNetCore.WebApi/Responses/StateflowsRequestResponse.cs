using Stateflows.Common;
using Stateflows.Common.Classes;

namespace Stateflows.Transport.AspNetCore.WebApi.Responses
{
    internal class StateflowsRequestResponse : StateflowsSendResponse
    {
        public StateflowsRequestResponse(SendResult result, Response response) : base(result)
        {
            Response = response;
        }

        public object Response { get; set; }
    }
}
