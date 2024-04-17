using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class UnsubscriptionHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(UnsubscriptionRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is UnsubscriptionRequest request)
            {
                var result = context.StateMachine.GetExecutor().Context.Context.RemoveSubscribers(request.BehaviorId, request.NotificationNames);

                request.Respond(new UnsubscriptionResponse() { UnsubscriptionSuccessful = result });

                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
