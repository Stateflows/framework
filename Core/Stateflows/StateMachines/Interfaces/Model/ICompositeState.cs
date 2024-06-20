using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public interface ICompositeState : IBaseState
    {
        Task OnInitializeAsync()
            => Task.CompletedTask;

        Task OnFinalizeAsync()
            => Task.CompletedTask;
    }
}
