using System.Threading.Tasks;
using Stateflows.StateMachines.Registration.Interfaces;

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

    public interface ICompositeStateDefinition : ICompositeState
    {
        void Build(ICompositeStateBuilder builder);
    }
}
