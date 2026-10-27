namespace Stateflows.StateMachines
{
    /// <summary>Built-in regular state type available for use as an initial state in a typed definition.</summary>
    public sealed class InitialState : IState
    {
        /// <summary>The registered name used by typed builder overloads.</summary>
        public static string Name => State<InitialState>.Name;
    }
}
