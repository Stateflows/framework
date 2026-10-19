using Stateflows.Common;

namespace Stateflows.Entities
{
    /// <summary>
    /// Provides the entity, mutation event, and executing behavior to an observer.
    /// </summary>
    public interface IMutationObservationContext<out TTemplate, out TMutationEvent> :
        IMutationContext<TTemplate, TMutationEvent>,
        IBehaviorActionContext
    {
    }
}
