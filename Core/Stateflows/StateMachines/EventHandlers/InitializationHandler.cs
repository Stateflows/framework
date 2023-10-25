using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class InitializationHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(InitializationRequest);

        public async Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is InitializationRequest)
            {
                var executor = context.StateMachine.GetExecutor();

                var initialized = await executor.InitializeAsync(context.Event as InitializationRequest);

                if (!initialized)
                {
                    Debug.WriteLine("not initialized");
                }

                (context.Event as InitializationRequest).Respond(new InitializationResponse() { InitializationSuccessful = initialized });

                return EventStatus.Consumed;
            }

            return EventStatus.NotConsumed;
        }
    }
}
