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
                throw new StateflowsException($"Already responded to request '{Name}'");
            }

            Response = response;
        }

        public TResponse Response { get; private set; }
    }

    public class Request<TRequestPayload, TResponsePayload> : Request<Response<TResponsePayload>>
    {
        public Request()
        {
            Payload = default;
        }

        public TRequestPayload Payload { get; set; }
    }
}
