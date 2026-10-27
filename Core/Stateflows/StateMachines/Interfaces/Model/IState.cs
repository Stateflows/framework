using System;
using System.Threading.Tasks;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Marks a state or pseudostate that can be used as a typed vertex in a machine definition.</summary>
    public interface IVertex : IStateMachineElement;

    /// <summary>Marks a regular state whose entry and exit actions may be supplied by additional interfaces.</summary>
    public interface IState : IVertex;

    /// <summary>Supplies an action awaited whenever the state is entered.</summary>
    public interface IStateEntry : IState
    {
        /// <summary>Executes the state's entry action before its nested configuration is entered.</summary>
        /// <returns>A task representing completion of the entry action.</returns>
        Task OnEntryAsync();
    }

    /// <summary>Supplies an action awaited whenever the state is exited.</summary>
    public interface IStateExit : IState
    {
        /// <summary>Executes the state's exit action after nested active states have exited.</summary>
        /// <returns>A task representing completion of the exit action.</returns>
        Task OnExitAsync();
    }

    /// <summary>Uses one abstract action implementation for both state entry and state exit.</summary>
    public interface IStateAction : IAbstractAction, IStateEntry, IStateExit
    {
        /// <inheritdoc/>
        Task IStateEntry.OnEntryAsync()
            => ExecuteAsync();

        /// <inheritdoc/>
        Task IStateExit.OnExitAsync()
            => ExecuteAsync();
    }

    /// <summary>Supplies the fluent definition of a typed regular state.</summary>
    /// <remarks>See <see href="https://github.com/Stateflows/framework/wiki/state-machine-state">states</see>.</remarks>
    public interface IStateDefinition : IState
    {
        /// <summary>Configures this state's actions, transitions, and deferrals during machine registration.</summary>
        /// <param name="builder">The builder scoped to this state.</param>
        static abstract void Build(IStateBuilder builder);
    }

    /// <summary>Marks a final vertex that completes its enclosing region when entered.</summary>
    public interface IFinalState : IVertex;

    /// <summary>Marks a history pseudostate that restores an enclosing state's remembered configuration.</summary>
    public interface IHistory : IVertex;
    
    /// <summary>Supplies a predicate that decides whether the current event should be deferred by a state.</summary>
    /// <typeparam name="TEvent">The candidate event type.</typeparam>
    public interface IDeferralGuard<in TEvent> : IAbstractGuard<TEvent>;

    /// <summary>Supplies an event-independent deferral predicate through the abstract guard contract.</summary>
    public interface IDeferralGuard : IAbstractGuard, IDeferralGuard<object>;

    /// <summary>Shares a typed predicate implementation between deferral rules and transition guards.</summary>
    /// <typeparam name="TEvent">The event type accepted by the predicate.</typeparam>
    public interface IStateMachineGuard<in TEvent> : IDeferralGuard<TEvent>, ITransitionGuard<TEvent>;
    
    /// <summary>Shares an event-independent predicate between deferral rules and transition guards.</summary>
    public interface IStateMachineGuard : IStateMachineGuard<object>, IDeferralGuard, ITransitionGuard;

    /// <summary>Resolves the registered name used for a typed state or pseudostate.</summary>
    /// <typeparam name="TState">The vertex type whose readable name is resolved.</typeparam>
    public static class State<TState>
        where TState : class, IVertex
    {
        /// <summary>The readable state name used by typed builder overloads.</summary>
        public static string Name => State.GetName(typeof(TState));
    }

    /// <summary>Resolves vertex type names consistently with fluent registration.</summary>
    public static class State
    {
        /// <summary>Resolves a CLR vertex type to its readable state-machine state name.</summary>
        /// <param name="stateType">The state or pseudostate type.</param>
        /// <returns>The readable name used when registering or targeting the type.</returns>
        public static string GetName(Type stateType) => stateType.GetReadableName(TypedElements.StateMachineStates);
    }
}
