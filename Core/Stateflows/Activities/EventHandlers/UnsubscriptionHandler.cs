using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class UnsubscriptionHandler : IActivityEventHandler
    {
        public Type EventType => typeof(UnsubscriptionRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is UnsubscriptionRequest request)
            {
                var result = context.Activity.GetExecutor().Context.Context.RemoveSubscriber(request.BehaviorId, request.EventName);

                request.Respond(new UnsubscriptionResponse() { UnsubscriptionSuccessful = result });

                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
