using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Events;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class ExecutionHandler : IActivityEventHandler
    {
        public Type EventType => typeof(ExecutionRequest);

        public async Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is ExecutionRequest executionRequest)
            {
                var executor = context.Activity.GetExecutor();

                var initialized = await executor.InitializeAsync(executionRequest.InitializationRequest ?? new InitializationRequest());

                executionRequest.Respond(
                    new ExecutionResponse()
                    {
                        ExecutionSuccessful = initialized,
                        OutputTokens = initialized
                            ? await executor.GetResultAsync()
                            : null
                    }
                );

                return EventStatus.Consumed;
            }

            return EventStatus.NotConsumed;
        }
    }
}
