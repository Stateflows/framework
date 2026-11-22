using System;
using System.Threading.Tasks;

namespace Stateflows.Entities
{
    /// <summary>
    /// Reports exceptions from entity initialization, mutations, computed fields, and projections.
    /// </summary>
    /// <remarks>
    /// All handlers are awaited in registration order, global handlers first.
    /// Their results are combined to indicate whether any handler handled the exception.
    /// Handling does not recover a value or commit partial state: execution still fails
    /// and is rolled back. The original exception remains available to observers and
    /// in the execution result. Failures during initialization also invoke the
    /// initialization hook, in addition to the failing operation's hook.
    /// A true result acknowledges the exception. IDE break-on-thrown settings control
    /// debugger stops independently of acknowledgement; a stop at the original throw
    /// occurs before these handlers run.
    /// </remarks>
    public interface IEntityExceptionHandler
    {
        /// <summary>
        /// Handles a failure of explicit or implicit entity initialization.
        /// </summary>
        /// <param name="initializationContext">The entity behavior and initialization-failure context.</param>
        /// <param name="exception">The exception thrown during initialization.</param>
        /// <returns>True to acknowledge the exception; false otherwise. Neither result recovers execution.</returns>
        Task<bool> OnEntityInitializationExceptionAsync(
            IEntityInitializationExceptionContext initializationContext,
            Exception exception);

        /// <summary>
        /// Handles an exception thrown while executing a registered mutation.
        /// </summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TMutationEvent">The mutation event type.</typeparam>
        /// <param name="mutationContext">The entity, mutation event, and behavior context.</param>
        /// <param name="exception">The exception thrown by the mutation operation.</param>
        /// <returns>True to acknowledge the exception; false otherwise. Neither result recovers execution.</returns>
        Task<bool> OnMutationExceptionAsync<TTemplate, TMutationEvent>(
            IMutationObservationContext<TTemplate, TMutationEvent> mutationContext,
            Exception exception);

        /// <summary>
        /// Handles an exception thrown while evaluating a computed field.
        /// </summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TField">The computed field value type.</typeparam>
        /// <param name="computedFieldContext">The entity, field name, and behavior context.</param>
        /// <param name="exception">The exception thrown by the field computation.</param>
        /// <returns>True to acknowledge the exception; false otherwise. Neither result recovers execution.</returns>
        Task<bool> OnComputedFieldExceptionAsync<TTemplate, TField>(
            IComputedFieldObservationContext<TTemplate> computedFieldContext,
            Exception exception);

        /// <summary>
        /// Handles an exception thrown while evaluating a projection.
        /// </summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TProjection">The projection value type.</typeparam>
        /// <param name="projectionContext">The entity, publication scope, and behavior context.</param>
        /// <param name="exception">The exception thrown by the projection operation.</param>
        /// <returns>True to acknowledge the exception; false otherwise. Neither result recovers execution.</returns>
        Task<bool> OnProjectionExceptionAsync<TTemplate, TProjection>(
            IProjectionObservationContext<TTemplate> projectionContext,
            Exception exception);
    }
}
