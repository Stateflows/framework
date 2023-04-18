using System.Threading;

namespace Stateflows.Common.Classes
{
    internal class EventHolder
    {
        public BehaviorId TargetId { get; private set; }

        public Event Event { get; private set; }

        public EventWaitHandle Handled { get; } = new EventWaitHandle(false, EventResetMode.AutoReset);

        public bool Consumed { get; set; }

        public EventHolder(BehaviorId targetId, Event @event)
        {
            TargetId = targetId;
            Event = @event;
        }
    }
}
