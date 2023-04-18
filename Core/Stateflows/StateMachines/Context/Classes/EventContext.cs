using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class EventContext<TEvent> : BaseContext, IEventContext<TEvent>, IEventInspectionContext<TEvent>
        where TEvent : Event
    {
        IStateMachineContext IEventContext<TEvent>.StateMachine => StateMachine;
        IStateMachineInspectionContext IEventInspectionContext<TEvent>.StateMachine => StateMachine;

        public EventContext(RootContext context) : base(context)
        { }

        public TEvent Event
        {
            get
            {
                if (!Context.Context.Values.TryGetValue(Constants.Event, out var @event))
                {
                    Debug.Assert(true, "Event object is not available. Is context set up properly?");
                }

                return @event as TEvent;
            }
        }
    }
}
