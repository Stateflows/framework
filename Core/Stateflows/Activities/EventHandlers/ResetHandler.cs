using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class ResetHandler : IActivityEventHandler
    {
        public Type EventType => typeof(Reset);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)

        {
            if (context.Event is Reset request)
            {
                context.Behavior.GetExecutor().Reset(request.Mode);

                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
