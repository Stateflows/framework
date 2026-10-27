using Stateflows.Common;

namespace Stateflows.StateMachines
{
    /// <summary>Describes a state machine's lifecycle status, identity, properties, and active-state hierarchy.</summary>
    [Retain, StrictOwnership]
    public class StateMachineInfo : BehaviorInfo
    {
        private StateMachineId id;

        /// <summary>Gets or sets the state-machine identifier, also updating the base behavior identifier.</summary>
        public new StateMachineId Id
        {
            get => id;
            set
            {
                id = value;
                base.Id = value;
            }
        }
        
        /// <summary>Gets or sets the hierarchy of current state names, including active nested states.</summary>
        public IReadOnlyTree<string> CurrentStates { get; set; }

        /// <summary>Determines whether the current-state tree should be serialized.</summary>
        /// <returns><see langword="true"/> when the current-state tree has a root.</returns>
        public bool ShouldSerializeCurrentStates()
            => CurrentStates is { Root: not null };
    }
}
