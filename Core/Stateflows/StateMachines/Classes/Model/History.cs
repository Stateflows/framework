namespace Stateflows.StateMachines
{
    /// <summary>Built-in typed history pseudostate for restoring a previously active nested configuration.</summary>
    public sealed class History : IHistory
    {
        /// <summary>The registered name used by typed builder overloads.</summary>
        public static string Name => State<History>.Name;
    }
}
