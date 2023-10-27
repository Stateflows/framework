using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class InitializationHandler : IActivityEventHandler
    {
        public Type EventType => typeof(InitializationRequest);

        public async Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is InitializationRequest)
            {
                var executor = context.Activity.GetExecutor();

                var initialized = await executor.InitializeAsync(context.Event as InitializationRequest);

                (context.Event as InitializationRequest).Respond(new InitializationResponse() { InitializationSuccessful = initialized });

                return EventStatus.Consumed;
            }

            return EventStatus.NotConsumed;
        }
    }
}
