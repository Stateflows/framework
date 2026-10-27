using Stateflows.Common;

namespace Stateflows.Entities
{
    /// <summary>
    /// Provides the entity, mutation event, and executing behavior to an observer.
    /// </summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <typeparam name="TMutationEvent">The event type being applied.</typeparam>
    public interface IMutationObservationContext<out TTemplate, out TMutationEvent> :
        IMutationContext<TTemplate, TMutationEvent>,
        IBehaviorActionContext
    {
    }
}
