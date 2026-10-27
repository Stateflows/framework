namespace Stateflows.StateMachines
{
    /// <summary>Built-in typed final vertex that completes its containing region.</summary>
    public sealed class FinalState : IFinalState
    {
        /// <summary>The registered name used by typed builder overloads.</summary>
        public static string Name => State<FinalState>.Name;
    }
}
