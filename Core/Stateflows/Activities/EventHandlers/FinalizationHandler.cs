using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class FinalizationHandler : IActivityEventHandler
    {
        public Type EventType => typeof(Finalize);

        public async Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)

        {
            if (context.Event is Finalize request)
            {
                var finalized = await context.Behavior.GetExecutor().CancelAsync();

                return finalized
                    ? EventStatus.Consumed
                    : EventStatus.Rejected;
            }

            return EventStatus.NotConsumed;
        }
    }
}
