using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class TokensOutputHandler : IActivityEventHandler
    {
        public Type EventType => typeof(TokensOutputRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)
        {
            if (context.Event is TokensOutputRequest request)
            {
                var result = context.Behavior.GetExecutor().Context.ActivityOutputTokens;

                request.Respond(new TokensOutput() { Tokens = result });
                
                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
