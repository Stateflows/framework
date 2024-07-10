using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public abstract class BaseState
    {
        [Obsolete("Context is deprecated, use dependency injection services (IExecutionContext, IStateContext, IStateMachineContext, IBehaviorLocator) or value accesors (GlobalValue, StateValue)")]
        public IStateActionContext Context { get; internal set; }

        public virtual Task OnEntryAsync()
            => Task.CompletedTask;

        public virtual Task OnExitAsync()
            => Task.CompletedTask;
    }

    public static class StateInfo<TState>
        where TState : BaseState
    {
        public static string Name => typeof(TState).FullName;
    }
}
