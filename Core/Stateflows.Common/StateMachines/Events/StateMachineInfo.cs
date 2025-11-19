using Stateflows.Common;

namespace Stateflows.StateMachines
{
    [Retain, StrictOwnership]
    public class StateMachineInfo : BehaviorInfo
    {
        private StateMachineId id;

        public new StateMachineId Id
        {
            get => id;
            set
            {
                id = value;
                base.Id = value;
            }
        }
        
        public IReadOnlyTree<string> CurrentStates { get; set; }
    }
}
