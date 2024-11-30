using Stateflows.Common;

namespace Stateflows.StateMachines.Events
{
    public sealed class StateMachineInfo : BehaviorInfo
    {
        public IReadOnlyTree<string> StatesTree { get; set; }
    }
}
