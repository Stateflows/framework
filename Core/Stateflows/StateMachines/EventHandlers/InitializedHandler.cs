using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class InitializedHandler : IStateMachineEventHandler
    {
        public string EventName => EventInfo<InitializedRequest>.Name;

        public Task<bool> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is InitializedRequest)
            {
                (context.Event as InitializedRequest).Respond(new InitializedResponse() { Initialized = context.StateMachine.GetExecutor().Initialized });

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}
