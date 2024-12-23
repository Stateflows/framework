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

        private IVertexContext currentVertex = null;
        public IVertexContext CurrentState => currentVertex ??= new VertexContext(Vertex, Context);
    }
}
