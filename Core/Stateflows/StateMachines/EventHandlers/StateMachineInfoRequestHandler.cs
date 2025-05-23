using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class StateMachineInfoRequestHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(StateMachineInfoRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)
        {
            if (context.Event is StateMachineInfoRequest request)
            {
                var executor = context.Behavior.GetExecutor();

                var response = new StateMachineInfo()
                {
                    Id = executor.Context.Id,
                    CurrentStates = executor.GetStatesTree(),
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
