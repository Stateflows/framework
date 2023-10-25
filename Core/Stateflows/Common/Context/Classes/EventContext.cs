using System;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Common.Context.Classes
{
    internal class EventContext<TEvent> : BaseContext, IEventContext<TEvent>
        where TEvent : Event, new()
    {
        IBehaviorContext IBehaviorActionContext.Behavior => Behavior;

        public EventContext(StateflowsContext context, IServiceProvider serviceProvider, TEvent @event)
            : base(context, serviceProvider)
        {
            Event = @event;
        }

        public TEvent Event { get; }
    }
}
