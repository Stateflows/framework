using System;
using System.Threading.Tasks;

namespace Stateflows.Entities
{
    /// <summary>
    /// Observes execution of entity mutations, computed fields, and projections.
    /// </summary>
    /// <remarks>
    /// Global observers wrap local observers, in registration order within each group.
    /// Each hook must call and await its continuation exactly once, propagate its
    /// exception, and leave entity state and returned values unchanged.
    /// Continuation results are exposed for inspection, not replacement.
    /// Use try/finally for observation that must also run when execution fails.
    /// The runtime rejects missing or repeated continuation calls, drains unfinished
    /// continuations, and preserves continuation failures even if an observer catches them.
    /// Observers remain responsible for leaving entity state and returned objects unchanged.
    /// </remarks>
    public interface IEntityObserver
    {
        /// <summary>
        /// Wraps execution of a registered mutation.
        /// </summary>
        Task OnMutationAsync<TTemplate, TMutationEvent>(
            IMutationObservationContext<TTemplate, TMutationEvent> context,
            Func<Task> next);

        /// <summary>
        /// Wraps each evaluation of a computed field, including initialization
        /// and dependency-driven recalculation.
        /// </summary>
        Task OnComputedFieldAsync<TTemplate, TField>(
            IComputedFieldObservationContext<TTemplate> context,
            Func<Task<TField>> next);

        /// <summary>
        /// Wraps each evaluation of a projection, including initialization
        /// and dependency-driven recalculation, but not reads of cached values.
        /// </summary>
        Task OnProjectionAsync<TTemplate, TProjection>(
            IProjectionObservationContext<TTemplate> context,
            Func<Task<TProjection>> next);
    }
}
