using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public abstract class State
    {
        public IStateContext CurrentState { get; internal set; }

        public IStateMachineContext StateMachine { get; internal set; }

        public virtual Task OnEntryAsync()
            => Task.CompletedTask;

        public virtual Task OnExitAsync()
            => Task.CompletedTask;
    }

    public sealed class StateInfo<TState>
        where TState : State
    {
        public static string Name { get => typeof(TState).Name; }
    }
}
