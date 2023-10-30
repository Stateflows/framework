using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class ResetHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(ResetRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is ResetRequest request)
            {
                var executor = context.StateMachine.GetExecutor();

                executor.Reset();

                request.Respond(new ResetResponse() { ResetSuccessful = true });

                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
