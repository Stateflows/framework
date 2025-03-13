using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class UnsubscriptionHandler : IActivityEventHandler
    {
        public Type EventType => typeof(Unsubscribe);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)

        {
            if (context.Event is Unsubscribe request)
            {
                var result = context.Behavior.GetExecutor().Context.Context.RemoveSubscribers(request.BehaviorId, request.NotificationNames);

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
