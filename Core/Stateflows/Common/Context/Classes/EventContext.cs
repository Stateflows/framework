using System;
using System.Collections.Generic;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Common.Context.Classes
{
    internal class EventContext<TEvent> : BaseContext, IEventContext<TEvent>
    {
        IBehaviorContext IBehaviorActionContext.Behavior => Behavior;

        public EventContext(StateflowsContext context, IServiceProvider serviceProvider, EventHolder<TEvent> eventHolder)
            : base(context, serviceProvider)
        {
            Event = eventHolder.Payload;
            EventId = eventHolder.Id;
            Headers = eventHolder.Headers;
        }

        public TEvent Event { get; }
        public Guid EventId { get; }
        public IEnumerable<EventHeader> Headers { get; }
        public object ExecutionTrigger => Event;
        public Guid ExecutionTriggerId => EventId; 
    }
}
