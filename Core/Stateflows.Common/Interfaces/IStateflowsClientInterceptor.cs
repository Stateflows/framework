using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface IStateflowsClientInterceptor
    {
        Task<bool> BeforeDispatchEventAsync<TEvent>(TEvent @event, List<EventHeader> headers);
        Task AfterDispatchEventAsync<TEvent>(TEvent @event);
    }
}
