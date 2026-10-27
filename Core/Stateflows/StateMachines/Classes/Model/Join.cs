namespace Stateflows.StateMachines
{
    /// <summary>Built-in typed join pseudostate for synchronizing incoming orthogonal-region paths.</summary>
    public sealed class Join : IVertex
    {
        /// <summary>The registered name used by typed builder overloads.</summary>
        public static string Name => State<Join>.Name;
    }
}
