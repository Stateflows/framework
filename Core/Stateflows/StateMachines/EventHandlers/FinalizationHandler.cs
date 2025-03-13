using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class FinalizationHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(Finalize);

        public async Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)
        {
            if (context.Event is Finalize request)
            {
                var finalized = await context.Behavior.GetExecutor().ExitAsync();

                return EventStatus.Consumed;
            }

            return EventStatus.NotConsumed;
        }
    }
}
