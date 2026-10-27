using System.Threading.Tasks;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Marks a state containing a nested region with its own initial and final configuration.</summary>
    public interface ICompositeState : IState;

    /// <summary>Supplies entry behavior for a composite state.</summary>
    public interface ICompositeStateEntry : ICompositeState, IStateEntry;
    
    /// <summary>Supplies exit behavior for a composite state.</summary>
    public interface ICompositeStateExit : ICompositeState, IStateExit;

    /// <summary>Supplies initialization behavior for a composite state's nested region.</summary>
    public interface ICompositeStateInitialization : ICompositeState
    {
        /// <summary>Executes when the composite state's nested configuration is initialized.</summary>
        /// <returns>A task representing completion of state initialization.</returns>
        Task OnInitializeAsync();
    }

    /// <summary>Supplies finalization behavior for a composite state's nested region.</summary>
    public interface ICompositeStateFinalization : ICompositeState
    {
        /// <summary>Executes when the composite state's nested configuration is finalized.</summary>
        /// <returns>A task representing completion of state finalization.</returns>
        Task OnFinalizeAsync();
    }
    
    /// <summary>Uses a single abstract action for entry, exit, initialization, and finalization.</summary>
    public interface ICompositeStateAction :
        IStateAction,
        ICompositeStateEntry,
        ICompositeStateExit,
        ICompositeStateInitialization,
        ICompositeStateFinalization
    {
        /// <inheritdoc/>
        Task ICompositeStateInitialization.OnInitializeAsync()
            => ExecuteAsync();

        /// <inheritdoc/>
        Task ICompositeStateFinalization.OnFinalizeAsync()
            => ExecuteAsync();
    }

    /// <summary>Supplies the fluent definition of a composite state and its nested region.</summary>
    /// <remarks>See <see href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state">composite states</see>.</remarks>
    public interface ICompositeStateDefinition : ICompositeState
    {
        /// <summary>Configures the composite state's lifecycle, transitions, and nested vertices.</summary>
        /// <param name="builder">The composite-state builder scoped to this definition.</param>
        static abstract void Build(ICompositeStateBuilder builder);
    }
}
