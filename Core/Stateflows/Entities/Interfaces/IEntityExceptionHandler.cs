using System;
using System.Threading.Tasks;

namespace Stateflows.Entities
{
    /// <summary>
    /// Handles exceptions from entity mutations, computed fields, and projections.
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
        /// <param name="context">The entity behavior and initialization-failure context.</param>
        /// <param name="exception">The exception thrown during initialization.</param>
        /// <returns>True if the exception was handled; otherwise, false.</returns>
        Task<bool> OnEntityInitializationExceptionAsync(
            IEntityInitializationExceptionContext context,
            Exception exception);

        /// <summary>
        /// Handles an exception thrown while executing a registered mutation.
        /// </summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TMutationEvent">The mutation event type.</typeparam>
        /// <param name="context">The entity, mutation event, and behavior context.</param>
        /// <param name="exception">The exception thrown by the mutation operation.</param>
        /// <returns>True if the exception was handled; otherwise, false.</returns>
        Task<bool> OnMutationExceptionAsync<TTemplate, TMutationEvent>(
            IMutationObservationContext<TTemplate, TMutationEvent> context,
            Exception exception);

        /// <summary>
        /// Handles an exception thrown while evaluating a computed field.
        /// </summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TField">The computed field value type.</typeparam>
        /// <param name="context">The entity, field name, and behavior context.</param>
        /// <param name="exception">The exception thrown by the field computation.</param>
        /// <returns>True if the exception was handled; otherwise, false.</returns>
        Task<bool> OnComputedFieldExceptionAsync<TTemplate, TField>(
            IComputedFieldObservationContext<TTemplate> context,
            Exception exception);

        /// <summary>
        /// Handles an exception thrown while evaluating a projection.
        /// </summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TProjection">The projection value type.</typeparam>
        /// <param name="context">The entity, publication scope, and behavior context.</param>
        /// <param name="exception">The exception thrown by the projection operation.</param>
        /// <returns>True if the exception was handled; otherwise, false.</returns>
        Task<bool> OnProjectionExceptionAsync<TTemplate, TProjection>(
            IProjectionObservationContext<TTemplate> context,
            Exception exception);
    }
}
