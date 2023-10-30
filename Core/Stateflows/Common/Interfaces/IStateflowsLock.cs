using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsLock
    {
        Task LockAsync(BehaviorId id);
        Task UnlockAsync(BehaviorId id);
    }
}
