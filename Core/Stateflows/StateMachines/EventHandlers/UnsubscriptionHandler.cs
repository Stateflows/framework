using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class UnsubscriptionHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(Unsubscribe);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)

        {
            if (context.Event is Unsubscribe request)
            {
                var result = context.Behavior.GetExecutor().Context.Context.RemoveSubscribers(request.BehaviorId, request.NotificationNames);

                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
