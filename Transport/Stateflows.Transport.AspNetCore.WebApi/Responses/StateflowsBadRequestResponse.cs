namespace Stateflows.Transport.AspNetCore.WebApi.Responses
{
    internal class StateflowsBadRequestResponse
    {
        public StateflowsBadRequestResponse(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public string ErrorMessage { get; }
    }
}
