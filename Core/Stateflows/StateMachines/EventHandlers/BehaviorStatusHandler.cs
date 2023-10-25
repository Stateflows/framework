using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class BehaviorStatusHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(BehaviorStatusRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is BehaviorStatusRequest)
            {
                (context.Event as BehaviorStatusRequest).Respond(new BehaviorStatusResponse() { BehaviorStatus = context.StateMachine.GetExecutor().BehaviorStatus });

                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
