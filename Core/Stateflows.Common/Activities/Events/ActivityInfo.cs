using Stateflows.Common;

namespace Stateflows.Activities
{
    /// <summary>Describes an activity's lifecycle status, identity, properties, and active-node hierarchy.</summary>
    [Retain, StrictOwnership]
    public sealed class ActivityInfo : BehaviorInfo
    {
        private ActivityId id;

        /// <summary>Gets or sets the activity identifier, also updating the base behavior identifier.</summary>
        public new ActivityId Id
        {
            get => id;
            set
            {
                id = value;
                base.Id = value;
            }
        }
        
        /// <summary>Gets or sets the hierarchy of active activity node names.</summary>
        public IReadOnlyTree<string> ActiveNodes { get; set; }

        /// <summary>Determines whether an active-node root is available for conditional serialization.</summary>
        /// <returns><see langword="true"/> when the active-node tree has a root.</returns>
        public bool ShouldSerializeCurrentStates()
            => ActiveNodes is { Root: not null };
    }
}
