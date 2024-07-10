using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class InitializeHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(Initialize);

        public async Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is Initialize)
            {
                return EventStatus.NotInitialized;
            }

            return EventStatus.NotConsumed;
        }
    }
}
