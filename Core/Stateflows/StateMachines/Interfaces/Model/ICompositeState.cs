using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public interface ICompositeState : IState
    { }

    public interface ICompositeStateEntry : ICompositeState, IStateEntry
    { }
    
    public interface ICompositeStateExit : ICompositeState, IStateExit
    { }

    public interface ICompositeStateInitialization : ICompositeState
    {
        Task OnInitializeAsync();
    }

    public interface ICompositeStateFinalization : ICompositeState
    {
        Task OnFinalizeAsync();
    }
}
