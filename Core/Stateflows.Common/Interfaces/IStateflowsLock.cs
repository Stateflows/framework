using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsLock
    {
        Task Lock(BehaviorId id);
        Task Unlock(BehaviorId id);
    }
}
