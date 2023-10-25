using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class EventContext<TEvent> : BaseContext, IEventInspectionContext<TEvent>, IRootContext
        where TEvent : Event, new()
    {
        IStateMachineContext IStateMachineActionContext.StateMachine => StateMachine;

        IStateMachineInspectionContext IEventInspectionContext<TEvent>.StateMachine => StateMachine;

        public EventContext(RootContext context) : base(context)
        { }

        public TEvent Event => Context.Event as TEvent;
    }
}
