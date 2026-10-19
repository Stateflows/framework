using Stateflows.Common;

namespace Stateflows.Entities
{
    /// <summary>
    /// Describes the behavior and event that triggered a failed initialization.
    /// </summary>
    public interface IEntityInitializationExceptionContext : IBehaviorActionContext
    {
        /// <summary>
        /// Gets whether initialization was triggered implicitly by an incoming event.
        /// </summary>
        bool ImplicitInitialization { get; }
    }
}
