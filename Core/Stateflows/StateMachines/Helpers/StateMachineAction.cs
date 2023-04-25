using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public static class StateMachineAction
    {
        public static Func<IStateMachineActionContext, Task> ToAsync(this Action<IStateMachineActionContext> stateMachineAction)
            => c => Task.Run(() => stateMachineAction(c));
    }
}
