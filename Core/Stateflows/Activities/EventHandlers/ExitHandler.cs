using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Events;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Inspection.Interfaces;
using Stateflows.Common.Events;

namespace Stateflows.Activities.EventHandlers
{
    internal class ExitHandler : IActivityEventHandler
    {
        public Type EventType => typeof(Exit);

        public async Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is Exit)
            {
                var executor = context.Activity.GetExecutor();

                await executor.ExitAsync();
                //context.Event.Respond(new CancelResponse() { Cancelled = executor.Cancel(executor.Graph) });

                return EventStatus.Consumed;
            }

            return EventStatus.NotConsumed;
        }
    }
}
