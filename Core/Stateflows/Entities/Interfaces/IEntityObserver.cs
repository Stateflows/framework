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
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TMutationEvent">The mutation event type.</typeparam>
        /// <param name="context">The entity, triggering event, behavior, and delivery context.</param>
        /// <param name="next">The continuation that executes the mutation and must be called exactly once.</param>
        Task OnMutationAsync<TTemplate, TMutationEvent>(
            IMutationObservationContext<TTemplate, TMutationEvent> context,
            Func<Task> next);

        /// <summary>
        /// Wraps each evaluation of a computed field, including initialization
        /// and dependency-driven recalculation.
        /// </summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TField">The computed field value type.</typeparam>
        /// <param name="context">The entity, field name, behavior, and delivery context.</param>
        /// <param name="next">The continuation that calculates the field value and must be called exactly once.</param>
        Task OnComputedFieldAsync<TTemplate, TField>(
            IComputedFieldObservationContext<TTemplate> context,
            Func<Task<TField>> next);

        /// <summary>
        /// Wraps each evaluation of a projection, including initialization
        /// and dependency-driven recalculation, but not reads of cached values.
        /// </summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TProjection">The projection value type.</typeparam>
        /// <param name="context">The entity, publication scope, behavior, and delivery context.</param>
        /// <param name="next">The continuation that calculates the projection and must be called exactly once.</param>
        Task OnProjectionAsync<TTemplate, TProjection>(
            IProjectionObservationContext<TTemplate> context,
            Func<Task<TProjection>> next);
    }
}
