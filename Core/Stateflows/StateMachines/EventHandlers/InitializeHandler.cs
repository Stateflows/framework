using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class InitializeHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(Initialize);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            => Task.FromResult(
                context.Event is Initialize
                    ? EventStatus.NotInitialized
                    : EventStatus.NotConsumed
            );
    }
}
