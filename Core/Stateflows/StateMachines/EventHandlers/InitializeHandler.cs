using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class InitializeHandler : IStateMachineEventHandler
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
