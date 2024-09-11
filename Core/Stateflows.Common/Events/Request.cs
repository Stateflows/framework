using Stateflows.Common.Exceptions;

namespace Stateflows.Common
{
    public abstract class Request<TResponse> : Event
        where TResponse : Event, new()
    {
        public void Respond(TResponse response)
        {
            if (Response != null)
            {
                throw new StateflowsRuntimeException($"Already responded to request '{Name}'");
            }

            Response = response;
        }

        public TResponse Response { get; private set; }
    }
}
