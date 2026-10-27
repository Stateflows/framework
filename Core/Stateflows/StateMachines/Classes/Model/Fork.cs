namespace Stateflows.StateMachines
{
    /// <summary>Built-in typed fork pseudostate for activating multiple orthogonal regions.</summary>
    public sealed class Fork : IVertex
    {
        /// <summary>The registered name used by typed builder overloads.</summary>
        public static string Name => State<Fork>.Name;
    }
}
