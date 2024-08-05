using Stateflows.Common.Exceptions;

namespace Stateflows.Common
{
    public abstract class Request<TResponse>
    {
        public void Respond(TResponse response)
        {
            if (Response != null)
            {
                throw new StateflowsException($"Already responded to request '{EventInfo.GetName(GetType())}'");
            }

            ResponseHolder = new EventHolder<TResponse>() { Payload = response };
        }

        public EventHolder<TResponse> ResponseHolder { get; private set; }

        public TResponse Response => ResponseHolder.Payload;
    }
}
