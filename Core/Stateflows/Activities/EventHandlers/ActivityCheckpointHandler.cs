using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal sealed class ActivityCheckpointHandler : IActivityEventHandler
    {
        public Type EventType => typeof(ActivityCheckpoint);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)
            => Task.FromResult(context.Event is ActivityCheckpoint ? EventStatus.Consumed : EventStatus.NotConsumed);
    }
}
