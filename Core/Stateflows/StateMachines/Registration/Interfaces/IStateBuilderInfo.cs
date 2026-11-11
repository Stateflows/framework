namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Exposes a configured vertex's registered name and structural kind.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.1.-State-Identity" />
    public interface IStateBuilderInfo
    {
        /// <summary>The vertex's registered name.</summary>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.1.-State-Identity" />
        string Name { get; }
        /// <summary>The state or pseudostate kind used by validation and execution.</summary>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.1.-State-Identity" />
        VertexType Type { get; }
    }
}