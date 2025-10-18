using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class SetContextOwnerHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(SetContextOwner);
        public Task<EventStatus> TryHandleEventAsync<TEvent>(Context.Interfaces.IEventContext<TEvent> context)
        {
            if (context.Event is SetContextOwner @event)
            {
                ((EventContext<TEvent>)context).Context.Context.ContextOwnerId = @event.ContextOwnerId;
                ((EventContext<TEvent>)context).Context.Context.ContextParentId = @event.ContextParentId;
                
                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
