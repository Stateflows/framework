using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class FinalizationHandler : IActivityEventHandler
    {
        public Type EventType => typeof(Finalize);

        public async Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)
        {
            var executor = context.Behavior.GetExecutor();
            if (executor.BehaviorStatus == BehaviorStatus.Initialized && context.Event is Finalize request)
            {
                var finalized = await executor.CancelAsync();

                return finalized
                    ? EventStatus.Consumed
                    : EventStatus.Rejected;
            }

            return EventStatus.NotConsumed;
        }
    }
}
