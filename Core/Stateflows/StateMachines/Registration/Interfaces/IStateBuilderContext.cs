namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Provides registration-time state identity and containment to definition extensions.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
    public interface IStateBuilderContext
    {
        /// <summary>The registered name of the state being configured.</summary>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        string StateName { get; }

        /// <summary>Attempts to resolve the containing state's registration context.</summary>
        /// <param name="stateBuilderDetails">The containing state's context when nested.</param>
        /// <returns>Whether a containing state exists.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        bool TryGetParent(out IStateBuilderContext stateBuilderDetails);
    }
}
