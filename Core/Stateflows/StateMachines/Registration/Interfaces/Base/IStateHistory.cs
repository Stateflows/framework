namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateHistory<out TReturn>
    {
        /// <summary>
        /// Adds history entrypoint to current composite/orthogonal state.
        /// </summary>
        /// <param name="historyEntrypointName">History entrypoint name</param>
        /// <param name="buildAction">Build action that enables configuration of default history mechanism</param>
        TReturn AddHistory(string historyEntrypointName, HistoryBuildAction buildAction = null);
        
        /// <summary>
        /// Adds history entrypoint to current composite/orthogonal state.
        /// </summary>
        /// <param name="buildAction">Build action that enables configuration of default history mechanism</param>
        TReturn AddHistory(HistoryBuildAction buildAction = null)
            => AddHistory(State<History>.Name, buildAction);
    }
}
