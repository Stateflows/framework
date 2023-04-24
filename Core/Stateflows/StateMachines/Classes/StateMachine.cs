using Stateflows.StateMachines.Context.Interfaces;
using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public abstract class StateMachine
    {
        public IStateMachineActionContext Context { get; internal set; }

        public virtual Task OnInitializeAsync()
            => Task.CompletedTask;

        public abstract void Build(ITypedStateMachineInitialBuilder builder);
    }

    public sealed class StateMachineInfo<TStateMachine>
        where TStateMachine : StateMachine
    {
        public static string Name { get => typeof(TStateMachine).Name; }
    }
}
