using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Events;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class ExitHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(ExitEvent);

        public async Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is ExitEvent)
            {
                var executor = context.StateMachine.GetExecutor();

                await executor.ExitAsync();

                return EventStatus.Consumed;
            }

            return EventStatus.NotConsumed;
        }
    }
}
