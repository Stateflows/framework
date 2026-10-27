namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Exposes a configured vertex's registered name and structural kind.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state-identity" />
    public interface IStateBuilderInfo
    {
        /// <summary>The vertex's registered name.</summary>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state-identity" />
        string Name { get; }
        /// <summary>The state or pseudostate kind used by validation and execution.</summary>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state-identity" />
        VertexType Type { get; }
    }
}