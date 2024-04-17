using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class NotificationsHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(NotificationsRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
            => Task.FromResult(context.Event is NotificationsRequest
                ? EventStatus.Consumed
                : EventStatus.NotConsumed
            );
    }
}
