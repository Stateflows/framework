using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public interface IBaseCompositeState : IBaseState
    { }

    public interface ICompositeStateEntry : IBaseCompositeState
    {
        Task OnEntryAsync();
    }

    public interface ICompositeStateExit : IBaseCompositeState
    {
        Task OnExitAsync();
    }

    public interface ICompositeStateInitialization : IBaseCompositeState
    {
        Task OnInitializeAsync();
    }

    public interface ICompositeStateFinalization : IBaseCompositeState
    {
        Task OnFinalizeAsync();
    }
}
