using Stateflows.Common;

namespace Stateflows.StateMachines
{
    public sealed class StateMachineInfo : BehaviorInfo
    {
        public IReadOnlyTree<string> StatesTree { get; set; }
    }
}
