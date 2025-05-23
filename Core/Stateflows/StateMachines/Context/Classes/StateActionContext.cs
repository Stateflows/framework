using Stateflows.Common;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class StateActionContext : BaseContext, IStateActionContext, IRootContext
    {
        public Vertex Vertex { get; set; }

        public string ActionName { get; }

        public StateActionContext(RootContext context, Vertex vertex, string name)
            : base(context)
        {
            Vertex = vertex;
            ActionName = name;
        }

        private IStateContext state = null;
        IStateMachineContext IStateMachineActionContext.StateMachine => StateMachine;
        public IStateContext State => state ??= new StateContext(Vertex, Context);
        public IBehaviorContext Behavior => StateMachine;
    }
}
