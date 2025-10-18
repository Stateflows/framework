using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class StateMachineActionContext : BaseContext, IStateMachineActionContext, IRootContext
    {
        public StateMachineActionContext(RootContext context)
            : base(context)
        { }

        public IReadOnlyTree<IStateContext> CurrentStates => StateMachine.CurrentStates;
        
        public IBehaviorContext Behavior => StateMachine;
        
        public bool TryGetStateContext(string stateName, out IStateContext stateContext)
            => StateMachine.TryGetStateContext(stateName, out stateContext);
    }
}
