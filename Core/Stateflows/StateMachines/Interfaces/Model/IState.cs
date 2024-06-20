using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public interface IBaseState
    {
        Task OnEntryAsync()
            => Task.CompletedTask;

        Task OnExitAsync()
            => Task.CompletedTask;
    }

    public interface IState : IBaseState
    { }

    public interface IFinalState : IBaseState
    { }

    public static class State<TState>
        where TState : class, IBaseState
    {
        public static string Name => typeof(TState).FullName;
    }
}
