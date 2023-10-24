using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface IBehaviorClientInterceptor
    {
        Task<bool> BeforeDispatchEventAsync(Event @event);
        Task AfterDispatchEventAsync(Event @event);
    }
}
