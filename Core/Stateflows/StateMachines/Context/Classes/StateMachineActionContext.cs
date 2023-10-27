using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class StateMachineActionContext : BaseContext, IStateMachineActionContext, IStateMachineActionInspectionContext
    {
        IStateMachineContext IStateMachineActionContext.StateMachine => StateMachine;

        IStateMachineInspectionContext IStateMachineActionInspectionContext.StateMachine => StateMachine;

        public StateMachineActionContext(RootContext context)
            : base(context)
        { }
    }
}
