namespace Stateflows.StateMachines
{
    /// <summary>Built-in typed choice pseudostate for selecting a guarded outgoing path after incoming effects.</summary>
    public sealed class Choice : IVertex
    {
        /// <summary>The registered name used by typed builder overloads.</summary>
        public static string Name => State<Choice>.Name;
    }
}
