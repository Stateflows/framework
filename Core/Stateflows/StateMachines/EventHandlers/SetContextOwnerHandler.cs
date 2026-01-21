using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Extensions;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class SetContextOwnerHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(SetContextOwner);
        public Task<EventStatus> TryHandleEventAsync<TEvent>(Context.Interfaces.IEventContext<TEvent> context)
        {
            if (context.Event is SetContextOwner @event)
            {
                context.Behavior.GetExecutor().Reset(ResetMode.Full);

                var stateflowsContext = ((EventContext<TEvent>)context).Context.Context;
                
                stateflowsContext.Deleted = false;

                stateflowsContext.ContextOwnerId = @event.ContextOwnerId;
                stateflowsContext.ContextParentId = @event.ContextParentId;
                
                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
