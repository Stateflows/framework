namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Adds a history entrypoint so a remembered nested configuration can be restored on re-entry.</summary>
    /// <typeparam name="TReturn">The enclosing builder returned after history registration.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-history" />
    public interface IStateHistory<out TReturn>
    {
        /// <summary>Adds a history entrypoint for restoring a remembered nested state configuration.</summary>
        /// <param name="historyEntrypointName">Registered name of the history entrypoint within the machine or region.
        /// </param>
        /// <param name="buildAction">Optional callback configuring the fallback path used when no remembered state
        /// configuration exists.</param>
        /// <returns>The enclosing region or state builder after adding the history entrypoint.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-history" />
        TReturn AddHistory(string historyEntrypointName, HistoryBuildAction buildAction = null);
        
        /// <summary>Adds a history entrypoint for restoring a remembered nested state configuration.</summary>
        /// <param name="buildAction">Optional callback configuring the fallback path used when no remembered state
        /// configuration exists.</param>
        /// <returns>The enclosing region or state builder after adding the history entrypoint.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-history" />
        TReturn AddHistory(HistoryBuildAction buildAction = null)
            => AddHistory(State<History>.Name, buildAction);
    }
}
