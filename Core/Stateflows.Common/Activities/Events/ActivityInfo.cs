using Stateflows.Common;

namespace Stateflows.Activities
{
    public sealed class ActivityInfo : BehaviorInfo
    {
        private ActivityId id;

        public new ActivityId Id
        {
            get => id;
            set
            {
                id = value;
                base.Id = value;
            }
        }
        
        public IReadOnlyTree<string> ActiveNodes { get; set; }
    }
}
