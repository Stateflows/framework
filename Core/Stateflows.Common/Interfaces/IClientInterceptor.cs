using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface IClientInterceptor
    {
        Task<bool> BeforeDispatchEventAsync(Event @event);
        Task AfterDispatchEventAsync(Event @event);
    }
}
