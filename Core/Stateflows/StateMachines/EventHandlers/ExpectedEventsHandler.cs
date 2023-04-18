using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class ExpectedEventsHandler : IStateMachineEventHandler
    {
        public string EventName => EventInfo<ExpectedEventsRequest>.Name;

        public async Task<bool> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is ExpectedEventsRequest)
            {
                (context.Event as ExpectedEventsRequest).Respond(new ExpectedEventsResponse() {
                    ExpectedEvents = (await context.StateMachine.GetExecutor().GetExpectedEventsAsync())
                        .Where(type => !type.IsSubclassOf(typeof(TimeEvent)))
                        .Where(type => type != typeof(Completion))
                        .Select(type => type.GetEventName())
                });

                return true;
            }

            return false;
        }
    }
}
