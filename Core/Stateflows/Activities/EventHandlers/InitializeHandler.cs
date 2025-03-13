using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class InitializeHandler : IActivityEventHandler
    {
        public Type EventType => typeof(Initialize);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)

            => Task.FromResult(
                context.Event is Initialize
                    ? EventStatus.NotInitialized
                    : EventStatus.NotConsumed
            );
    }
}
