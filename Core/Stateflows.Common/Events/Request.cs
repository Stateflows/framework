using System;

namespace Stateflows.Common
{
    public abstract class Request<TResponse> : Event
        where TResponse : Response, new()
    {
        public void Respond(TResponse response)
        {
            if (Response != null)
            {
                throw new Exception($"Already responded to request '{Name}'");
            }

            Response = response;
        }

        public TResponse Response { get; private set; }
    }
}
