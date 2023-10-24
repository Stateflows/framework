using Stateflows.Common;

namespace Stateflows.Transport.AspNetCore.WebApi.Responses
{
    internal class StateflowsSendResponse
    {
        public StateflowsSendResponse(SendResult result)
        {
            Status = result.Status;
            Validation = result.Validation;
        }

        public EventStatus Status { get; }

        public EventValidation Validation { get; }
    }
}
