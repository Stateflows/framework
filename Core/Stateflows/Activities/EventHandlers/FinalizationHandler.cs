using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class FinalizationHandler : IActivityEventHandler
    {
        public Type EventType => typeof(FinalizationRequest);

        public async Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is FinalizationRequest request)
            {
                var finalized = await context.Activity.GetExecutor().CancelAsync();

                request.Respond(new FinalizationResponse() { FinalizationSuccessful = finalized });

                return EventStatus.Consumed;
            }

            return EventStatus.NotConsumed;
        }
    }
}
