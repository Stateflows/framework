using Stateflows.Common;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class GuardContext<TEvent> : TransitionContext<TEvent>, IGuardInspectionContext<TEvent>
    {
        public GuardContext(RootContext context, Edge edge) : base(context, edge)
        { }

        IStateMachineInspectionContext IGuardInspectionContext<TEvent>.StateMachine => StateMachine;
    }
}
