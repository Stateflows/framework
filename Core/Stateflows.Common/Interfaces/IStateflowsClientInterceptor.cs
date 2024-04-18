using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface IStateflowsClientInterceptor
    {
        Task<bool> BeforeDispatchEventAsync(object @event);
        Task AfterDispatchEventAsync(object @event);
    }
}
