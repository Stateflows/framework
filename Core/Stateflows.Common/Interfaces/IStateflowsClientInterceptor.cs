using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface IStateflowsClientInterceptor
    {
        Task<bool> BeforeDispatchEventAsync(Event @event);
        Task AfterDispatchEventAsync(Event @event);
    }
}
