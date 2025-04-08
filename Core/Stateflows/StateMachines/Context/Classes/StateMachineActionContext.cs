using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class StateMachineActionContext : BaseContext, IStateMachineActionContext, IRootContext
    {
        public StateMachineActionContext(RootContext context)
            : base(context)
        { }

        IStateMachineContext IStateMachineActionContext.StateMachine => StateMachine;
        
        public IBehaviorContext Behavior => StateMachine;
    }
}
