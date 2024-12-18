using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Activities.Events;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Inspection.Interfaces;
using Stateflows.StateMachines;

namespace Stateflows.Activities.EventHandlers
{
    internal class TypedTokensOutputHandler : IActivityEventHandler
    {
        public Type EventType => typeof(TokensOutputRequest<>);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
        {
            var eventType = context.Event.GetType();
            Debug.WriteLine("--------------> dupa");
            if (eventType.IsGenericType && eventType.GetGenericTypeDefinition() == typeof(TokensOutputRequest<>))
            {
                var tokenType = eventType.GetGenericArguments().First();
                
                var result = context.Activity.GetExecutor().Context.ActivityOutputTokens
                    .Where(tokenHolder => tokenHolder.PayloadType == tokenType).ToArray();

                var responseType = typeof(TokensOutput<>).MakeGenericType(tokenType);
                var response = Activator.CreateInstance(responseType) as TokensOutputEvent;
                response!.Tokens.AddRange(result);
                ResponseHolder.Respond(context.Event, response.ToTypedEventHolder());
                // typeof(IRequest<>)
                //     .GetMethod("Respond")!
                //     .MakeGenericMethod(responseType)
                //     .Invoke(context.Event, new object[] { response });
                
                return Task.FromResult(EventStatus.Consumed);
            }

            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
