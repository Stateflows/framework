using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class BehaviorStatusHandler : IActivityEventHandler
    {
        public Type EventType => typeof(BehaviorInfoRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)

        {
            if (context.Event is BehaviorInfoRequest request)
            {
                var executor = context.Behavior.GetExecutor();

                request.Respond(new BehaviorInfo()
                {
                    BehaviorStatus = executor.BehaviorStatus,
                    ExpectedEvents = executor.GetExpectedEventNames(),
                });

                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
