using System;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Context.Interfaces;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class EventContext<TEvent> : BaseContext, Interfaces.IEventContext<TEvent>, IRootContext
    {
        public EventContext(RootContext context) : base(context)
        {
            Event = default;

            if (context.EventHolder is EventHolder<TEvent> holder)
            {
                Event = holder.Payload;
            }
            else
            {
                if (context.EventHolder.BoxedPayload is TEvent @event)
                {
                    Event = @event;
                }
                else
                {
                    if (ImplicitConverter.TryConvert<TEvent>(context.EventHolder.BoxedPayload, out var convertedEvent))
                    {
                        Event = convertedEvent;
                    }
                    else
                    {
                        throw new StateflowsRuntimeException(
                            $"Failed to convert event of type {context.EventHolder.BoxedPayload.GetType()} to {typeof(TEvent)}");
                    }
                }
            }
        }

        public TEvent Event { get; private set; }

        public Guid EventId => Context.EventHolder.Id;

        public override IEnumerable<EventHeader> Headers => Context.EventHolder.Headers;

        IStateMachineContext IStateMachineActionContext.StateMachine => StateMachine;
        
        public IBehaviorContext Behavior => StateMachine;
    }
}
