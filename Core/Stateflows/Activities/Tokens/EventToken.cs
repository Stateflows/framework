using Stateflows.Common;

namespace Stateflows.Activities
{
    public class EventToken<TEvent> : Token
        where TEvent : Event, new()
    {
        public TEvent Event { get; set; }
    }
}
