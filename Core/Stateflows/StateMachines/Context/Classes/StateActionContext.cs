using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class StateActionContext : BaseContext,
        IStateActionInspectionContext,
        IRootContext
    {
        public Vertex Vertex { get; set; }

        public string ActionName { get; }

        IStateMachineContext IStateMachineActionContext.StateMachine => StateMachine;
        IStateMachineInspectionContext IStateActionInspectionContext.StateMachine => StateMachine;

        public StateActionContext(RootContext context, Vertex vertex, string name)
            : base(context)
        {
            Vertex = vertex;
            ActionName = name;
        }

        private IStateContext currentState = null;
        public IStateContext CurrentState => currentState ??= new StateContext(Vertex, Context);
    }
}
