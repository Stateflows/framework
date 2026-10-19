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
    /// </remarks>
    public interface IEntityExceptionHandler
    {
        /// <summary>
        /// Handles a failure of explicit or implicit entity initialization.
        /// </summary>
        /// <returns>True if the exception was handled; otherwise, false.</returns>
        Task<bool> OnEntityInitializationExceptionAsync(
            IEntityInitializationExceptionContext context,
            Exception exception);

        /// <summary>
        /// Handles an exception thrown while executing a registered mutation.
        /// </summary>
        /// <returns>True if the exception was handled; otherwise, false.</returns>
        Task<bool> OnMutationExceptionAsync<TTemplate, TMutationEvent>(
            IMutationObservationContext<TTemplate, TMutationEvent> context,
            Exception exception);

        /// <summary>
        /// Handles an exception thrown while evaluating a computed field.
        /// </summary>
        /// <returns>True if the exception was handled; otherwise, false.</returns>
        Task<bool> OnComputedFieldExceptionAsync<TTemplate, TField>(
            IComputedFieldObservationContext<TTemplate> context,
            Exception exception);

        /// <summary>
        /// Handles an exception thrown while evaluating a projection.
        /// </summary>
        /// <returns>True if the exception was handled; otherwise, false.</returns>
        Task<bool> OnProjectionExceptionAsync<TTemplate, TProjection>(
            IProjectionObservationContext<TTemplate> context,
            Exception exception);
    }
}
