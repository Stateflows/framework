namespace Stateflows.Transport.AspNetCore.WebApi.Responses
{
    internal class StateflowsSendResponse
    {
        public StateflowsSendResponse(bool consumed)
        {
            Consumed = consumed;
        }

        public bool Consumed { get; set; }
    }
}
