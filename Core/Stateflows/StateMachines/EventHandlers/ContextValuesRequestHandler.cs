using System;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class ContextValuesRequestHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(ContextValuesRequest);

        public async Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)
        {
            if (context.Event is ContextValuesRequest request)
            {
                request.Respond(new ContextValuesResponse()
                {
                    GlobalValues = context.Behavior.GetExecutor().Context.GlobalValues,
                    StateValues = context.Behavior.GetExecutor().Context.StateValues
                        .ToDictionary(e => e.Key, e => e.Value.Values)
                });

                return EventStatus.Consumed;
            }

            return EventStatus.NotConsumed;
        }
    }
}
