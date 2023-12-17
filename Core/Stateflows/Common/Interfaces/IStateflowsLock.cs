using System;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsLock
    {
        Task<IStateflowsLockHandle> AquireLockAsync(BehaviorId id, TimeSpan? timeout = null);
    }
}
