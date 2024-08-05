using System;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class BehaviorStatusRequestHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(BehaviorInfoRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is BehaviorInfoRequest request)
            {
                var executor = context.StateMachine.GetExecutor();

                request.Respond(new BehaviorStatus()
                {
                    BehaviorStatus = executor.BehaviorStatus,
                    ExpectedEvents = executor.GetExpectedEventNames(),
                });

                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
