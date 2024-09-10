using Stateflows.Common;

namespace Stateflows.Common.Classes
{
    public class EventToken
    {
        public BehaviorId TargetId { get; set; }

        public EventHolder Event { get; set; }
    }
}
