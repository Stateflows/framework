using System;
using System.Threading.Tasks;
using Stateflows.Common.Extensions;
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
        public static string Name => State.GetName(typeof(TState));
    }

    public static class State
    {
        public static string GetName(Type stateType) => stateType.GetReadableName(TypedElements.StateMachineStates);
    }
}
