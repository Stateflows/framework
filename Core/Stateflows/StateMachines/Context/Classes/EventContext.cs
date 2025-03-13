using System;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Context.Interfaces;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Utilities;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class EventContext<TEvent> : BaseContext, IEventInspectionContext<TEvent>, IRootContext

    {
        IStateMachineContext IStateMachineActionContext.StateMachine => StateMachine;

        IStateMachineInspectionContext IEventInspectionContext<TEvent>.StateMachine => StateMachine;

        public EventContext(RootContext context) : base(context)
        {
            Event = default;

            if (context.EventHolder is EventHolder<TEvent> holder)
            {
                Event = holder.Payload;
            }
            else
            {
                if (ImplicitConverter.TryConvert<TEvent>(context.EventHolder.BoxedPayload, out var @event))
                {
                    Event = @event;
                }
                else
                {
                    throw new StateflowsRuntimeException($"Failed to convert event of type {context.EventHolder.BoxedPayload.GetType()} to {typeof(TEvent)}");
                }
            }
        }

        public TEvent Event { get; private set; }

        public Guid EventId => Context.EventHolder.Id;

        public IEnumerable<EventHeader> Headers => Context.EventHolder.Headers;
        
        public IBehaviorContext Behavior => StateMachine;
    }
}
