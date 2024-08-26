using System;
using Stateflows.Common.Exceptions;

namespace Stateflows.Common
{
    public interface IRequest
    {
        EventHolder GeneralResponseHolder { get; }

        void Respond<TResponse>(TResponse response);
    }

    public abstract class Request<TResponse> : IRequest
    {
        public void Respond(TResponse response)
        {
            if (Response != null)
            {
                throw new StateflowsException($"Already responded to request '{EventInfo.GetName(GetType())}'");
            }

            ResponseHolder = new EventHolder<TResponse>() { Payload = response };
        }

        internal EventHolder<TResponse> ResponseHolder { get; private set; }

        public TResponse Response => ResponseHolder.Payload;

        EventHolder IRequest.GeneralResponseHolder => ResponseHolder;

        void IRequest.Respond<TResponseEvent>(TResponseEvent response)
        {
            if (response is TResponse typedResponse)
            {
                Respond(typedResponse);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
