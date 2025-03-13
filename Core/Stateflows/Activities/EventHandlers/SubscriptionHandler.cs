using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class SubscriptionHandler : IActivityEventHandler
    {
        public Type EventType => typeof(Subscribe);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)
        {
            if (context.Event is Subscribe request)
            {
                var result = context.Behavior.GetExecutor().Context.Context.AddSubscribers(request.BehaviorId, request.NotificationNames);

                return Task.FromResult(
                    result
                        ? EventStatus.Consumed
                        : EventStatus.Rejected
                );
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
