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
        public IStateContext State => state ??= new StateContext(Vertex, Context);
        public IReadOnlyTree<IStateContext> CurrentStates => StateMachine.CurrentStates;
        public IBehaviorContext Behavior => StateMachine;
        public bool TryGetStateContext(string stateName, out IStateContext stateContext)
            => StateMachine.TryGetStateContext(stateName, out stateContext);
    }
}
