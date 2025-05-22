using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    public class StateflowsNoLockHandle : IStateflowsLockHandle
    {
        public StateflowsNoLockHandle(BehaviorId behaviorId)
        {
            BehaviorId = behaviorId;
        }

        public BehaviorId BehaviorId { get; }

        public ValueTask DisposeAsync()
            => default;
    }
}