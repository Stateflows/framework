namespace Stateflows.Transport.AspNetCore.WebApi.Responses
{
    internal class StateflowsInitializedResponse
    {
        public StateflowsInitializedResponse(bool initialized)
        {
            Initialized = initialized;
        }

        public bool Initialized { get; set; }
    }
}
