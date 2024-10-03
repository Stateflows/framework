using System;
using System.Collections.Generic;
using Stateflows.Common;
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
                var @event = context.EventHolder.BoxedPayload;

                var converter = typeof(TEvent).GetMethod("op_Implicit", new[] { @event.GetType() });

                if (converter != null)
                {
                    @event = converter.Invoke(null, new[] { @event });
                }

                Event = (TEvent)@event;
            }
        }

        public TEvent Event { get; private set; }

        public Guid EventId => Context.EventHolder.Id;

        public IEnumerable<EventHeader> Headers => Context.EventHolder.Headers;
    }
}
