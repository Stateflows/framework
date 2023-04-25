using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public static class StateAction
    {
        public static Func<IStateActionContext, Task> ToAsync(this Action<IStateActionContext> stateAction)
            => c => Task.Run(() => stateAction(c));
    }
}
