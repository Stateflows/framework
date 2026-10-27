using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>
    /// Acknowledges activity failures without recovering execution. All handlers are awaited sequentially;
    /// returning true acknowledges the exception. Execution still fails and its context
    /// changes are not persisted. IDE break-on-thrown settings control debugger stops
    /// independently of acknowledgement. Graph exception-handler nodes are separate.
    /// </summary>
    public interface IActivityExceptionHandler
    {
        /// <summary>Observes an exception raised while initializing an activity.</summary>
        /// <param name="context">The initialization context.</param>
        /// <param name="exception">The exception thrown by initialization.</param>
        /// <returns><see langword="true"/> to acknowledge the failure; this does not recover execution.</returns>
        Task<bool> OnActivityInitializationExceptionAsync(IActivityInitializationContext context, Exception exception);

        /// <summary>Observes an exception raised while finalizing an activity.</summary>
        /// <param name="context">The finalization context.</param>
        /// <param name="exception">The exception thrown by finalization.</param>
        /// <returns><see langword="true"/> to acknowledge the failure; this does not recover execution.</returns>
        Task<bool> OnActivityFinalizationExceptionAsync(IActivityFinalizationContext context, Exception exception);

        /// <summary>Observes an exception raised while initializing a node.</summary>
        /// <param name="context">The node context.</param>
        /// <param name="exception">The exception thrown by node initialization.</param>
        /// <returns><see langword="true"/> to acknowledge the failure; this does not recover execution.</returns>
        Task<bool> OnNodeInitializationExceptionAsync(IActivityNodeContext context, Exception exception);

        /// <summary>Observes an exception raised while finalizing a node.</summary>
        /// <param name="context">The node context.</param>
        /// <param name="exception">The exception thrown by node finalization.</param>
        /// <returns><see langword="true"/> to acknowledge the failure; this does not recover execution.</returns>
        Task<bool> OnNodeFinalizationExceptionAsync(IActivityNodeContext context, Exception exception);

        /// <summary>Observes an exception raised while executing a node action.</summary>
        /// <param name="context">The node context.</param>
        /// <param name="exception">The exception thrown by the action.</param>
        /// <returns><see langword="true"/> to acknowledge the failure; this does not recover execution.</returns>
        Task<bool> OnNodeExecutionExceptionAsync(IActivityNodeContext context, Exception exception);

        /// <summary>Observes an exception raised while evaluating a flow guard.</summary>
        /// <typeparam name="TToken">The token type considered by the guard.</typeparam>
        /// <param name="context">The guard context.</param>
        /// <param name="exception">The exception thrown by the guard.</param>
        /// <returns><see langword="true"/> to acknowledge the failure; this does not recover execution.</returns>
        Task<bool> OnFlowGuardExceptionAsync<TToken>(IGuardContext<TToken> context, Exception exception);

        /// <summary>Observes an exception raised while transforming a flow token.</summary>
        /// <typeparam name="TToken">The input token type.</typeparam>
        /// <typeparam name="TTransformedToken">The transformed token type.</typeparam>
        /// <param name="context">The transformation context.</param>
        /// <param name="exception">The exception thrown by the transformation.</param>
        /// <returns><see langword="true"/> to acknowledge the failure; this does not recover execution.</returns>
        Task<bool> OnFlowTransformationExceptionAsync<TToken, TTransformedToken>(ITransformationContext<TToken> context, Exception exception);

    }
}
