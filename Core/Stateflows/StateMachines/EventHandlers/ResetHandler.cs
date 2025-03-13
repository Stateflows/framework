using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class ResetHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(Reset);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)
        {
            if (context.Event is Reset request)
            {
                var executor = context.Behavior.GetExecutor();
                if (executor.Initialized)
                {
                    executor.Reset(request.Mode);
                    
                    return Task.FromResult(EventStatus.Consumed);
                }

                return Task.FromResult(EventStatus.Rejected);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
