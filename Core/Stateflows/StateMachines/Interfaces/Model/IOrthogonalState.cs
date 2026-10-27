using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Marks a state whose regions have independent, concurrently active state configurations.</summary>
    public interface IOrthogonalState : ICompositeState;

    /// <summary>Supplies entry behavior for an orthogonal state, rather than for each region separately.</summary>
    public interface IOrthogonalStateEntry : IOrthogonalState, ICompositeStateEntry;
    
    /// <summary>Supplies exit behavior for the enclosing orthogonal state.</summary>
    public interface IOrthogonalStateExit : IOrthogonalState, ICompositeStateExit;

    /// <summary>Supplies initialization behavior for the orthogonal state's regions.</summary>
    public interface IOrthogonalStateInitialization : IOrthogonalState, ICompositeStateInitialization;

    /// <summary>Supplies finalization behavior for the orthogonal state's regions.</summary>
    public interface IOrthogonalStateFinalization : IOrthogonalState, ICompositeStateFinalization;

    /// <summary>Uses a single abstract action for all lifecycle operations of an orthogonal state.</summary>
    public interface IOrthogonalStateAction :
        ICompositeStateAction,
        IOrthogonalStateEntry,
        IOrthogonalStateExit,
        IOrthogonalStateInitialization,
        IOrthogonalStateFinalization;

    /// <summary>Supplies an orthogonal state's lifecycle, outgoing transitions, and region definitions.</summary>
    /// <remarks>See <see href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state">orthogonal states</see>.</remarks>
    public interface IOrthogonalStateDefinition : IOrthogonalState
    {
        /// <summary>Configures this orthogonal state during registration.</summary>
        /// <param name="builder">The builder scoped to this orthogonal state.</param>
        static abstract void Build(IOrthogonalStateBuilder builder);
    }
}
