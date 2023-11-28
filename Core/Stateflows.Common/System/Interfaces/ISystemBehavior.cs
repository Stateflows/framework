using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.System.Events;

namespace Stateflows.System
{
    public interface ISystemBehavior : IBehavior
    {
        Task<RequestResult<AvailableBehaviorClassesResponse>> GetAvailableBehaviorClassesAsync();

        Task<RequestResult<BehaviorInstancesResponse>> GetBehaviorInstancesAsync();
    }
}
