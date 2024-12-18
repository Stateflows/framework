using System;
using System.Threading.Tasks;
using Stateflows.Activities.Events;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class TokensOutputHandler : IActivityEventHandler
    {
        public Type EventType => typeof(TokensOutputRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
        {
            if (context.Event is TokensOutputRequest request)
            {
                var result = context.Activity.GetExecutor().Context.ActivityOutputTokens;

                request.Respond(new TokensOutput() { Tokens = result });
                
                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
