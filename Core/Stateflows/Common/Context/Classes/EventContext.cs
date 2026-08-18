using System;
using System.Collections.Generic;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Common.Context.Classes
{
    internal class EventContext<TEvent> : BaseContext, IEventContext<TEvent>
    {
        IBehaviorContext IBehaviorActionContext.Behavior => Behavior;
        public bool TryGetParentBehaviorContext(out IParentBehaviorContext parentBehaviorContext)
        {
            parentBehaviorContext = Behavior.Context.ContextParentId.HasValue
                ? Behavior
                : null;
            
            return parentBehaviorContext != null;
        }
        public bool TryGetOwnerBehaviorContext(out IOwnerBehaviorContext ownerBehaviorContext)
        {
            ownerBehaviorContext = Behavior.Context.ContextOwnerId.HasValue
                ? Behavior
                : null;
            
            return ownerBehaviorContext != null;
        }

        public EventContext(StateflowsContext context, IServiceProvider serviceProvider, EventHolder<TEvent> eventHolder)
            : base(context, serviceProvider)
        {
            Event = eventHolder.Payload;
            EventId = eventHolder.Id;
            Headers = eventHolder.Headers;
        }

        public TEvent Event { get; }
        public Guid EventId { get; }
        public IDictionary<string, EventHeader> Headers { get; }
        public object ExecutionTrigger => Event;
        public Guid ExecutionTriggerId => EventId; 
    }
}
