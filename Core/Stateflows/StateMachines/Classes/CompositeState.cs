using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public abstract class CompositeState : BaseState
    {
        public virtual Task OnInitializeAsync()
            => Task.CompletedTask;

        public virtual Task OnFinalizeAsync()
            => Task.CompletedTask;
    }

    public static class CompositeStateInfo<TCompositeState>
        where TCompositeState : CompositeState
    {
        public static string Name => typeof(TCompositeState).FullName;
    }
}
