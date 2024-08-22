using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public interface ICompositeState : IState
    { }

    public interface ICompositeStateEntry : ICompositeState
    {
        Task OnEntryAsync();
    }

    public interface ICompositeStateExit : ICompositeState
    {
        Task OnExitAsync();
    }

    public interface ICompositeStateInitialization : ICompositeState
    {
        Task OnInitializeAsync();
    }

    public interface ICompositeStateFinalization : ICompositeState
    {
        Task OnFinalizeAsync();
    }
}
