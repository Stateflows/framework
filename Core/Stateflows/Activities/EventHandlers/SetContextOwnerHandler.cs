using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
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
