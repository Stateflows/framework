using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class SubscriptionHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(SubscriptionRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is SubscriptionRequest request)
            {
                var result = context.StateMachine.GetExecutor().Context.Context.AddSubscriber(request.BehaviorId, request.NotificationName);

                request.Respond(new SubscriptionResponse() { SubscriptionSuccessful = result });

                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
