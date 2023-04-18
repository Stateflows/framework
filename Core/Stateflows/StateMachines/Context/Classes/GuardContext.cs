using Stateflows.Common;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class GuardContext<TEvent> : TransitionContext<TEvent>, IGuardContext<TEvent>, IGuardInspectionContext<TEvent>, IEdgeContext
        where TEvent : Event
    {
        public GuardContext(RootContext context, Edge edge) : base(context, edge)
        { }

        IStateMachineInspectionContext IGuardInspectionContext<TEvent>.StateMachine => StateMachine;
    }
}
