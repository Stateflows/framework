using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.System
{
    public interface ISystem
    {
        Task<RequestResult<AvailableBehaviorClassesResponse>> GetAvailableBehaviorClassesAsync();

        Task<RequestResult<BehaviorInstancesResponse>> GetBehaviorInstancesAsync();
    }
}
