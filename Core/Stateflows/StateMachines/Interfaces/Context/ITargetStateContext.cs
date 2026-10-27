
namespace Stateflows.StateMachines
{
    /// <summary>Provides the identity of a transition target.</summary>
    public interface ITargetStateContext
    {
        /// <summary>The registered target-state name.</summary>
        string Name { get; }
    }
}
