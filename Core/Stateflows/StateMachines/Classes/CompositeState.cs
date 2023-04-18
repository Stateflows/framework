using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public abstract class CompositeState : State
    {
        public virtual Task OnInitializeAsync()
            => Task.CompletedTask;
    }

    public sealed class CompositeStateInfo<TCompositeState>
        where TCompositeState : CompositeState
    {
        public static string Name { get => typeof(TCompositeState).Name; }
    }
}
