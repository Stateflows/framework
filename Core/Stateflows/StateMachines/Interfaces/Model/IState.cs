using System;
using System.Threading.Tasks;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IVertex : IStateMachineElement;

    public interface IState : IVertex;

    public interface IStateEntry : IState
    {
        Task OnEntryAsync();
    }

    public interface IStateExit : IState
    {
        Task OnExitAsync();
    }

    public interface IStateAction : IAbstractAction, IStateEntry, IStateExit
    {
        Task IStateEntry.OnEntryAsync()
            => ExecuteAsync();

        Task IStateExit.OnExitAsync()
            => ExecuteAsync();
    }

    public interface IStateDefinition : IState
    {
        static abstract void Build(IStateBuilder builder);
    }

    public interface IFinalState : IVertex;

    public interface IHistory : IVertex;
    
    public interface IDeferralGuard<in TEvent> : IAbstractGuard<TEvent>;

    public interface IDeferralGuard : IAbstractGuard, IDeferralGuard<object>;

    public interface IStateMachineGuard<in TEvent> : IDeferralGuard<TEvent>, ITransitionGuard<TEvent>;
    
    public interface IStateMachineGuard : IDeferralGuard, ITransitionGuard;

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
