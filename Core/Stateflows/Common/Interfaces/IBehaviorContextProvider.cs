using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface IBehaviorContextProvider
    {
        Task<(bool Success, IBehaviorContextHolder ContextHolder)> TryProvideAsync(BehaviorId behaviorId);
    }
}