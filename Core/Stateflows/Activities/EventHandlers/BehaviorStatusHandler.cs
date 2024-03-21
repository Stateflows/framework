using System;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Inspection.Interfaces;
using Stateflows.StateMachines.Events;

namespace Stateflows.Activities.EventHandlers
{
    internal class BehaviorStatusHandler : IActivityEventHandler
    {
        public Type EventType => typeof(BehaviorStatusRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is BehaviorStatusRequest request)
            {
                var executor = context.Activity.GetExecutor();

                request.Respond(new BehaviorStatusResponse()
                {
                    BehaviorStatus = executor.BehaviorStatus,
                    ExpectedEvents = executor.GetExpectedEvents()
                        .Where(type => !type.IsSubclassOf(typeof(TimeEvent)))
                        .Where(type => type != typeof(CompletionEvent))
                        .Select(type => type.GetEventName())
                        .ToArray(),
                });

                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
