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
        { }

        public TEvent Event => (Context.EventHolder as EventHolder<TEvent>).Payload;

        public Guid EventId => Context.EventHolder.Id;

        public IEnumerable<EventHeader> Headers => Context.EventHolder.Headers;
    }
}
