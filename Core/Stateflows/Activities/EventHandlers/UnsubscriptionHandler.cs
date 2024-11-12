using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class UnsubscriptionHandler : IActivityEventHandler
    {
        public Type EventType => typeof(Unsubscribe);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
        {
            if (context.Event is Unsubscribe request)
            {
                var result = context.Activity.GetExecutor().Context.Context.RemoveSubscribers(request.BehaviorId, request.NotificationNames);

                return Task.FromResult(
                    result
                        ? EventStatus.Consumed
                        : EventStatus.Rejected
                );
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
