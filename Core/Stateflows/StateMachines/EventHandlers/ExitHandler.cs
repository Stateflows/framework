using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Events;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class ExitHandler : IStateMachineEventHandler
    {
        public string EventName => EventInfo<Exit>.Name;

        public async Task<bool> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is Exit)
            {
                var executor = context.StateMachine.GetExecutor();

                await executor.ExitAsync();

                return true;
            }

            return false;
        }
    }
}
