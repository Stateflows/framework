using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Extensions;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class StartRelayHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(StartRelay);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)
        {
            if (context.Event is StartRelay request)
            {
                var result = context.Behavior.GetExecutor().Context.Context.AddRelays(request.BehaviorId, request.NotificationNames);

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
