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
        public bool TryGetParentBehaviorContext(out IParentBehaviorContext parentBehaviorContext)
        {
            parentBehaviorContext = StateMachine.Context.Context.ContextParentId.HasValue
                ? StateMachine.Behavior
                : null;
            
            return parentBehaviorContext != null;
        }
        public bool TryGetOwnerBehaviorContext(out IOwnerBehaviorContext ownerBehaviorContext)
        {
            ownerBehaviorContext = StateMachine.Context.Context.ContextParentId.HasValue
                ? StateMachine.Behavior
                : null;
            
            return ownerBehaviorContext != null;
        }
        
        public bool TryGetStateContext(string stateName, out IStateContext stateContext)
            => StateMachine.TryGetStateContext(stateName, out stateContext);
    }
}
