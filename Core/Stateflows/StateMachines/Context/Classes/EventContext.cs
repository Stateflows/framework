using System;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Exceptions;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class EventContext<TEvent> : BaseContext, IEventContext<TEvent>, IRootContext
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

        public override List<EventHeader> Headers => Context.EventHolder.Headers;

        public IReadOnlyTree<IStateContext> CurrentStates => StateMachine.CurrentStates;
        public bool TryGetStateContext(string stateName, out IStateContext stateContext)
            => StateMachine.TryGetStateContext(stateName, out stateContext);

        public IBehaviorContext Behavior => StateMachine;
    }
}
