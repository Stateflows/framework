using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class InitializationHandler : IStateMachineEventHandler
    {
        public string EventName => EventInfo<InitializationRequest>.Name;

        public async Task<bool> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is InitializationRequest)
            {
                var executor = context.StateMachine.GetExecutor();
                var initialized = false;
                if (!executor.Initialized)
                {
                    initialized = await executor.Initialize(context.Event as InitializationRequest);
                }

                (context.Event as InitializationRequest).Respond(new InitializationResponse() { InitializationSuccessful = initialized });

                return true;
            }

            return false;
        }
    }
}
