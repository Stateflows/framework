using System;
using System.Threading.Tasks;

namespace Stateflows.Entities
{
    /// <summary>
    /// Observes direct field writes, entity mutations, computed fields, and projections.
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
        /// Wraps an accepted direct <see cref="FieldState{T}"/> write,
        /// including dependency stabilization and dependent projection refresh.
        /// </summary>
        /// <remarks>
        /// Runs after field lookup, access checks, and validation, even when the value is unchanged.
        /// Does not run for writes performed by mutations, initializers, or computed fields,
        /// or for rejected or invalid write requests. Completion is not a durable commit.
        /// The default implementation transparently continues the write.
        /// </remarks>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TField">The declared field value type.</typeparam>
        /// <param name="fieldSetContext">The entity view, registered field name, requested value, and delivery context.</param>
        /// <param name="next">The remaining observers and direct write operation; call and await exactly once.</param>
        /// <returns>A task completing when observation and the write operation finish, or faulting on failure.</returns>
        Task OnFieldSetAsync<TTemplate, TField>(
            IFieldSetObservationContext<TTemplate, TField> fieldSetContext,
            Func<Task> next) => next();

        /// <summary>
        /// Wraps execution of a registered mutation.
        /// </summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TMutationEvent">The mutation event type.</typeparam>
        /// <param name="mutationContext">The entity, triggering event, behavior, and delivery context.</param>
        /// <param name="next">The continuation that executes the mutation and must be called exactly once.</param>
        /// <returns>A task completing when observation and mutation finish, or faulting on failure.</returns>
        Task OnMutationAsync<TTemplate, TMutationEvent>(
            IMutationObservationContext<TTemplate, TMutationEvent> mutationContext,
            Func<Task> next);

        /// <summary>
        /// Wraps each evaluation of a computed field, including initialization
        /// and dependency-driven recalculation.
        /// </summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TField">The computed field value type.</typeparam>
        /// <param name="computedFieldContext">The entity, field name, behavior, and delivery context.</param>
        /// <param name="next">The continuation that calculates the field value and must be called exactly once.</param>
        /// <returns>A task completing after the calculated value has been observed, or faulting on failure.</returns>
        Task OnComputedFieldAsync<TTemplate, TField>(
            IComputedFieldObservationContext<TTemplate> computedFieldContext,
            Func<Task<TField>> next);

        /// <summary>
        /// Wraps each evaluation of a projection, including initialization
        /// and dependency-driven recalculation, but not reads of cached values.
        /// </summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TProjection">The projection value type.</typeparam>
        /// <param name="projectionContext">The entity, publication scope, behavior, and delivery context.</param>
        /// <param name="next">The continuation that calculates the projection and must be called exactly once.</param>
        /// <returns>A task completing after the projection value has been observed, or faulting on failure.</returns>
        Task OnProjectionAsync<TTemplate, TProjection>(
            IProjectionObservationContext<TTemplate> projectionContext,
            Func<Task<TProjection>> next);
    }
}
