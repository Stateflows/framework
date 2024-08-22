using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public interface IVertex
    { }

    public interface IState : IVertex
    { }

    public interface IStateEntry : IState
    {
        Task OnEntryAsync();
    }

    public interface IStateExit : IState
    {
        Task OnExitAsync();
    }

    public interface IFinalState : IVertex
    { }

    public static class State<TState>
        where TState : class, IVertex
    {
        public static string Name => typeof(TState).FullName;
    }
}
