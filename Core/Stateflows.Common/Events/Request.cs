using System.Threading;
using System.Collections.Generic;

namespace Stateflows.Common
{
    public static class ResponseHolder
    {
        public readonly static AsyncLocal<Dictionary<IRequest, EventHolder>> Responses =
            new AsyncLocal<Dictionary<IRequest, EventHolder>>();
    }

    public interface IRequest
    { }

    public interface IRequest<in TResponse> : IRequest
    { }

    //public abstract class Request<TResponse> : IRequest
    //{
    //    public void Respond(TResponse response)
    //    {
    //        if (Response != null)
    //        {
    //            throw new StateflowsException($"Already responded to request '{EventInfo.GetName(GetType())}'");
    //        }

    //        ResponseHolder = new EventHolder<TResponse>() { Payload = response };
    //    }

    //    internal EventHolder<TResponse> ResponseHolder { get; private set; }

    //    public TResponse Response => ResponseHolder.Payload;

    //    EventHolder IRequest.GeneralResponseHolder => ResponseHolder;

    //    void IRequest.Respond<TResponseEvent>(TResponseEvent response)
    //    {
    //        if (response is TResponse typedResponse)
    //        {
    //            Respond(typedResponse);
    //        }
    //        else
    //        {
    //            throw new InvalidOperationException();
    //        }
    //    }
    //}
}
