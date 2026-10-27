using Stateflows.Common;
using Stateflows.Entities.Enums;

namespace Stateflows.Entities
{
    /// <summary>
    /// Provides the entity, configured publication scope, and executing behavior to an observer.
    /// </summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    public interface IProjectionObservationContext<out TTemplate> :
        IProjectionContext<TTemplate>,
        IBehaviorActionContext
    {
        /// <summary>
        /// Gets the publication targets configured for this projection.
        /// </summary>
        PublishScope PublishScope { get; }
    }
}
