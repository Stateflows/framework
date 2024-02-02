using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Events;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class CancelHandler : IActivityEventHandler
    {
        public Type EventType => typeof(CancelRequest);

        public async Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is CancelRequest cancelRequest)
            {
                var cancelled = await context.Activity.GetExecutor().CancelAsync();

                cancelRequest.Respond(new CancelResponse() { CancelSuccessful = cancelled });

                return EventStatus.Consumed;
            }

            return EventStatus.NotConsumed;
        }
    }
}
