using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class FinalizationHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(Finalize);

        public async Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is Finalize request)
            {
                var finalized = await context.StateMachine.GetExecutor().ExitAsync();

                request.Respond(new FinalizationResponse() { FinalizationSuccessful = finalized });

                return EventStatus.Consumed;
            }

            return EventStatus.NotConsumed;
        }
    }
}
