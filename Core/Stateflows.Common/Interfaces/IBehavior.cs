using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    public interface IBehavior
    {
        Task<bool> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event, new();

        Task<TResponse> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new();
    }
}