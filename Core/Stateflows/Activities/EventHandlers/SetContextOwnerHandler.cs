using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class SetContextOwnerHandler : IActivityEventHandler
    {
        public Type EventType => typeof(SetContextOwner);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)
        {
            if (context.Event is SetContextOwner @event)
            {
                ((EventContext<TEvent>)context).Context.Context.ContextOwnerId = @event.ContextOwner;
                
                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
