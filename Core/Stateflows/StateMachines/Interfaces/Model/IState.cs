using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Registration.Interfaces;

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

    public interface IStateDefinition : IState
    {
        void Build(IStateBuilder builder);
    }

    public interface IFinalState : IVertex
    { }

    public static class State<TState>
        where TState : class, IVertex
    {
        public static string Name => typeof(TState).FullName;
        public static string ShortName => typeof(TState).Name;
    }

    public static class State
    {
        public static string GetName(Type stateType) => stateType.FullName;
        public static string GetShortName(Type stateType) => stateType.Name;
    }
}
