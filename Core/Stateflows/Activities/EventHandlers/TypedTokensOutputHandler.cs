using System;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows.Activities.EventHandlers
{
    internal class TypedTokensOutputHandler : IActivityEventHandler
    {
        public Type EventType => typeof(TokensOutputRequest<>);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)
        {
            var eventType = context.Event.GetType();
            if (eventType.IsGenericType && eventType.GetGenericTypeDefinition() == typeof(TokensOutputRequest<>))
            {
                var tokenType = eventType.GetGenericArguments().First();
                
                var result = context.Behavior.GetExecutor().Context.ActivityOutputTokens
                    .Where(tokenHolder => tokenHolder.PayloadType == tokenType).ToArray();

                var responseType = typeof(TokensOutput<>).MakeGenericType(tokenType);
                var response = Activator.CreateInstance(responseType) as TokensOutputEvent;
                response!.Tokens.AddRange(result);
                ResponseHolder.Respond(context.Event, response.ToTypedEventHolder());
                
                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
