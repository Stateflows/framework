using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class ActivityInfoRequestHandler : IActivityEventHandler
    {
        public Type EventType => typeof(ActivityInfoRequest);

        public async Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)
        {
            if (context.Event is ActivityInfoRequest request)
            {
                var executor = context.Behavior.GetExecutor();

                var response = new ActivityInfo()
                {
                    Id = executor.Context.Id,
                    ActiveNodes = executor.GetNodesTree(),
                    ExpectedEvents = await executor.GetExpectedEventNamesAsync(),
                    BehaviorStatus = executor.BehaviorStatus
                };

                request.Respond(response);

                return EventStatus.Consumed;
            }

            return EventStatus.NotConsumed;
        }
    }
}
