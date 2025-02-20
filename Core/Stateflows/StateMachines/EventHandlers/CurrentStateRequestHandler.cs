using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class CurrentStateRequestHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(StateMachineInfoRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
        {
            if (context.Event is StateMachineInfoRequest request)
            {
                var executor = context.StateMachine.GetExecutor();

                var response = new StateMachineInfo()
                {
                    StatesTree = executor.GetStateTree(),
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
