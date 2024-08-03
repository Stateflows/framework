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

                if (executor.Graph.Interactive || executor.BehaviorStatus != BehaviorStatus.NotInitialized)
                {
                    return EventStatus.NotConsumed;
                }

                var status = await executor.InitializeAsync(executionRequest.InitializationEvent ?? new Initialize());

                executionRequest.Respond(
                    new ExecutionResponse()
                    {
                        //ExecutionStatus = status,
                        OutputTokens = status == EventStatus.Initialized
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
