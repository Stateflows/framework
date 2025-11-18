using System;
using System.Collections.Generic;
using Stateflows.Actions.Context.Interfaces;
using Stateflows.Actions.Engine;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Utilities;
using Stateflows.Common.Exceptions;

namespace Stateflows.Actions.Context.Classes
{
    internal class EventContext<TEvent> : ActionDelegateContext, IEventContext<TEvent>
    {
        public EventContext(StateflowsContext context, Executor executor, EventHolder eventHolder, IServiceProvider serviceProvider,
            List<TokenHolder> inputTokens = null)
            : base(context, executor, eventHolder, serviceProvider, inputTokens)
        {
            Event = default;
            
            if (RootContext.EventHolder is EventHolder<TEvent> holder)
            {
                Event = holder.Payload;
            }
            else
            {
                if (ImplicitConverter.TryConvert<TEvent>(RootContext.EventHolder.BoxedPayload, out var @event))
                {
                    Event = @event;
                }
                else
                {
                    throw new StateflowsRuntimeException($"Failed to convert event of type {RootContext.EventHolder.BoxedPayload.GetType()} to {typeof(TEvent)}");
                }
            }
        }

        public TEvent Event { get; }
    }
}