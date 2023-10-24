using Stateflows.Common.Classes;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    public interface IBehavior
    {
        BehaviorId Id { get; }

        Task<SendResult> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event;

        Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response;
    }
}