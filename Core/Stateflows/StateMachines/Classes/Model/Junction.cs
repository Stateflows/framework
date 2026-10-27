namespace Stateflows.StateMachines
{
    /// <summary>Built-in typed junction pseudostate for constructing guarded compound transition paths.</summary>
    public sealed class Junction : IVertex
    {
        /// <summary>The registered name used by typed builder overloads.</summary>
        public static string Name => State<Junction>.Name;
    }
}
