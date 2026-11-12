namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Provides registration-time state identity and containment to definition extensions.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.2.1.-State" />
    public interface IStateBuilderContext
    {
        /// <summary>The registered name of the state being configured.</summary>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.2.1.-State" />
        string StateName { get; }

        /// <summary>Attempts to resolve the containing state's registration context.</summary>
        /// <param name="stateBuilderDetails">The containing state's context when nested.</param>
        /// <returns>Whether a containing state exists.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.2.1.-State" />
        bool TryGetParent(out IStateBuilderContext stateBuilderDetails);
    }
}
