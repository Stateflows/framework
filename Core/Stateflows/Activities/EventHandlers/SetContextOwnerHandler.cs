using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class SetContextOwnerHandler : IActivityEventHandler
    {
        public Type EventType => typeof(SetContextOwner);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
        {
            if (context.Event is SetContextOwner @event)
            {
                ((EventContext<TEvent>)context).Context.Context.ContextOwner = @event.ContextOwner;
                
                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
