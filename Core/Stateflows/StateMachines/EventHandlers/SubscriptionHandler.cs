using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class SubscriptionHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(Subscribe);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)

        {
            if (context.Event is Subscribe request)
            {
                var result = context.Behavior.GetExecutor().Context.Context.AddSubscribers(request.BehaviorId, request.NotificationNames);

                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
