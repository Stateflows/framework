using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public interface IActivityContextProvider
    {
        Task<(bool Success, IActivityContextHolder ContextHolder)> TryProvideAsync(ActivityId activityId);
    }
}