using Stateflows.Common.Exceptions;

namespace Stateflows.Common
{
    public abstract class Request<TResponse> : Event
        where TResponse : Response, new()
    {
        public void Respond(TResponse response)
        {
            if (Response != null)
            {
                throw new StateflowsException($"Already responded to request '{EventName}'");
            }

            Response = response;
        }

        public TResponse Response { get; private set; }
    }
}
