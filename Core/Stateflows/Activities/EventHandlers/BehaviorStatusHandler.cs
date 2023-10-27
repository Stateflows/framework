using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class BehaviorStatusHandler : IActivityEventHandler
    {
        public Type EventType => typeof(BehaviorStatusRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is BehaviorStatusRequest)
            {
                var executor = context.Activity.GetExecutor();
                var status = (executor.Initialized, false) switch
                {
                    (false, false) => BehaviorStatus.NotInitialized,
                    (true, false) => BehaviorStatus.Initialized,
                    (true, true) => BehaviorStatus.Finalized,
                    _ => BehaviorStatus.NotInitialized
                };

                (context.Event as BehaviorStatusRequest).Respond(new BehaviorStatusResponse() { BehaviorStatus = status });

                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
