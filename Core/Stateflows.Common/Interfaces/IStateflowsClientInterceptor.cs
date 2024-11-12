using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface IStateflowsClientInterceptor
    {
        Task<bool> BeforeDispatchEventAsync(EventHolder eventHolder);
        Task AfterDispatchEventAsync(EventHolder eventHolder);
    }
}
