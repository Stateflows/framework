using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Extensions;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class StopRelayHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(StopRelay);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)

        {
            if (context.Event is StopRelay request)
            {
                var result = context.Behavior.GetExecutor().Context.Context.RemoveRelays(request.BehaviorId, request.NotificationNames);

                return Task.FromResult(
                    result
                        ? EventStatus.Consumed
                        : EventStatus.Rejected
                );
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
