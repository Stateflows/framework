using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public static class StateMachineAction
    {
        public static Func<IStateMachineInitializationContext, Task> ToAsync(this Action<IStateMachineInitializationContext> stateMachineAction)
            => c => Task.Run(() => stateMachineAction(c));

        public static Func<IStateMachineInitializationContext<TInitializationRequest>, Task> ToAsync<TInitializationRequest>(this Action<IStateMachineInitializationContext<TInitializationRequest>> stateMachineAction)
            where TInitializationRequest : InitializationRequest, new()
            => c => Task.Run(() => stateMachineAction(c));

        public static Func<IStateMachineActionContext, Task> ToAsync(this Action<IStateMachineActionContext> stateMachineAction)
            => c => Task.Run(() => stateMachineAction(c));
    }
}
