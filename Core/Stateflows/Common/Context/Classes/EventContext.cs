using System;
using System.Collections.Generic;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Common.Context.Classes
{
    internal class EventContext<TEvent>(
        StateflowsContext context,
        IServiceProvider serviceProvider,
        EventHolder<TEvent> eventHolder)
        : BaseContext(context, serviceProvider), IEventContext<TEvent>
    {
        IBehaviorContext IBehaviorActionContext.Behavior => Behavior;
        public bool TryGetParentBehaviorContext(out IParentBehaviorContext parentBehaviorContext)
        {
            parentBehaviorContext = Behavior.Context.ContextParentId.HasValue
                ? Behavior
                : null!;
            
            return parentBehaviorContext != null!;
        }
        public bool TryGetOwnerBehaviorContext(out IOwnerBehaviorContext ownerBehaviorContext)
        {
            ownerBehaviorContext = Behavior.Context.ContextOwnerId.HasValue
                ? Behavior
                : null!;
            
            return ownerBehaviorContext != null!;
        }

        public TEvent Event { get; } = eventHolder.Payload;
        public Guid EventId { get; } = eventHolder.Id;
        public IDictionary<string, EventHeader> Headers { get; } = eventHolder.Headers;
        public object ExecutionTrigger => Event;
        public Guid ExecutionTriggerId => EventId; 
    }
}
