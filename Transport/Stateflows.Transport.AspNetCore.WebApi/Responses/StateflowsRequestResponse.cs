namespace Stateflows.Transport.AspNetCore.WebApi.Responses
{
    internal class StateflowsRequestResponse : StateflowsSendResponse
    {
        public StateflowsRequestResponse(bool consumed, object response) : base(consumed)
        {
            Response = response;
        }

        public object Response { get; set; }
    }
}
