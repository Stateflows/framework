using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    /// <summary>A behavior lock handle that holds no physical lock and performs no work when disposed.</summary>
    public class StateflowsNoLockHandle : IStateflowsLockHandle
    {
        /// <summary>Creates a no-op handle for a behavior.</summary>
        /// <param name="behaviorId">The behavior associated with the handle.</param>
        public StateflowsNoLockHandle(BehaviorId behaviorId)
        {
            BehaviorId = behaviorId;
        }

        /// <inheritdoc />
        public BehaviorId BehaviorId { get; }

        /// <summary>Completes without releasing any physical lock.</summary>
        /// <returns>An already-completed asynchronous disposal operation.</returns>
        public ValueTask DisposeAsync()
            => default;
    }
}