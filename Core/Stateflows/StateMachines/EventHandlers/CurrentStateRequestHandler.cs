using System;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class CurrentStateRequestHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(CurrentStateRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)

        {
            if (context.Event is CurrentStateRequest request)
            {
                var executor = context.StateMachine.GetExecutor();

                var response = new CurrentState()
                {
                    StatesStack = executor.GetStateStack(),
                    ExpectedEvents = executor.GetExpectedEventNames(),
                    BehaviorStatus = executor.BehaviorStatus
                };

                request.Respond(response);

                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
