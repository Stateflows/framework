namespace Stateflows.Transport.AspNetCore.WebApi.Responses
{
    internal class StateflowsInitializeResponse
    {
        public StateflowsInitializeResponse(bool initializationSuccessful)
        {
            InitializationSuccessful = initializationSuccessful;
        }

        public bool InitializationSuccessful { get; set; }
    }
}
