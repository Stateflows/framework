using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class CurrentStateHandler : IStateMachineEventHandler
    {
        public string EventName => EventInfo<CurrentStateRequest>.Name;

        public async Task<bool> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is CurrentStateRequest)
            {
                (context.Event as CurrentStateRequest).Respond(new CurrentStateResponse() { CurrentState = await context.StateMachine.GetExecutor().GetCurrentStateAsync() });

                return true;
            }

            return false;
        }
    }
}
