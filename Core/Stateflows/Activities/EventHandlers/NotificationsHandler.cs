using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class NotificationsHandler : IActivityEventHandler
    {
        public Type EventType => typeof(NotificationsRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            => Task.FromResult(context.Event is NotificationsRequest
                ? EventStatus.Consumed
                : EventStatus.NotConsumed
            );
    }
}
