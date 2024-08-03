using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public interface IVertex
    { }

    public interface IBaseState : IVertex
    { }

    public interface IStateEntry : IBaseState
    {
        Task OnEntryAsync();
    }

    public interface IStateExit : IBaseState
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
