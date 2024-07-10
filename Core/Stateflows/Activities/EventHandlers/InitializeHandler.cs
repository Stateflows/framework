using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class InitializeHandler : IActivityEventHandler
    {
        public Type EventType => typeof(Initialize);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
            => Task.FromResult(
                context.Event is Initialize
                    ? EventStatus.NotInitialized
                    : EventStatus.NotConsumed
            );
    }
}
