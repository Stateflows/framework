using Stateflows.Common.Exceptions;

namespace Stateflows.Common
{
    public abstract class Request : Event
    {
        public void Respond(object response)
        {
            if (Response != null)
            {
                throw new StateflowsException($"Already responded to request '{Name}'");
            }

            Response = response;
        }

        public object Response { get; internal set; }
    }

    public abstract class Request<TResponse> : Request
        //where TResponse : Response, new()
    {
        public void Respond(TResponse response)
            => base.Respond(response);

        public new TResponse Response
        {
            get => (TResponse)base.Response;
            private set => base.Response = value;
        }
    }
}
