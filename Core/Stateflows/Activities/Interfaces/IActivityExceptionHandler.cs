using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>
    /// Receives notifications when activity, node, guard, or token-transformation
    /// execution fails. Handlers are awaited sequentially; returning <see langword="true"/>
    /// acknowledges the exception but does not recover execution. Activity context
    /// changes are not persisted after the failed execution. Failures thrown by a
    /// handler are recorded and logged without replacing the original execution
    /// failure. IDE break-on-thrown settings control debugger stops independently of
    /// acknowledgement, and graph exception-handler nodes are separate mechanisms.
    /// </summary>
    public interface IActivityExceptionHandler
    {
        /// <summary>Receives notification of a failure while initializing an activity.</summary>
        /// <param name="initializationContext">The context captured for the activity initialization that failed.</param>
        /// <param name="exception">The exception thrown during activity initialization.</param>
        /// <returns>A task yielding <see langword="true"/> to acknowledge the failure or <see langword="false"/> to leave it unacknowledged; neither value recovers execution.</returns>
        Task<bool> OnActivityInitializationExceptionAsync(IActivityInitializationContext initializationContext, Exception exception);

        /// <summary>Receives notification of a failure while finalizing an activity.</summary>
        /// <param name="finalizationContext">The context captured for the activity finalization that failed.</param>
        /// <param name="exception">The exception thrown during activity finalization.</param>
        /// <returns>A task yielding <see langword="true"/> to acknowledge the failure or <see langword="false"/> to leave it unacknowledged; neither value recovers execution.</returns>
        Task<bool> OnActivityFinalizationExceptionAsync(IActivityFinalizationContext finalizationContext, Exception exception);

        /// <summary>Receives notification of a failure while initializing a node.</summary>
        /// <param name="nodeContext">The context captured for the node initialization that failed.</param>
        /// <param name="exception">The exception thrown during node initialization.</param>
        /// <returns>A task yielding <see langword="true"/> to acknowledge the failure or <see langword="false"/> to leave it unacknowledged; neither value recovers execution.</returns>
        Task<bool> OnNodeInitializationExceptionAsync(IActivityNodeContext nodeContext, Exception exception);

        /// <summary>Receives notification of a failure while finalizing a node.</summary>
        /// <param name="nodeContext">The context captured for the node finalization that failed.</param>
        /// <param name="exception">The exception thrown during node finalization.</param>
        /// <returns>A task yielding <see langword="true"/> to acknowledge the failure or <see langword="false"/> to leave it unacknowledged; neither value recovers execution.</returns>
        Task<bool> OnNodeFinalizationExceptionAsync(IActivityNodeContext nodeContext, Exception exception);

        /// <summary>Receives notification of a failure while executing a node action.</summary>
        /// <param name="nodeContext">The context captured for the node action that failed.</param>
        /// <param name="exception">The exception thrown during node action execution.</param>
        /// <returns>A task yielding <see langword="true"/> to acknowledge the failure or <see langword="false"/> to leave it unacknowledged; neither value recovers execution.</returns>
        Task<bool> OnNodeExecutionExceptionAsync(IActivityNodeContext nodeContext, Exception exception);

        /// <summary>Receives notification of a failure while evaluating a flow guard.</summary>
        /// <typeparam name="TToken">The CLR token type considered by the guard.</typeparam>
        /// <param name="guardContext">The context captured for the flow-guard evaluation that failed.</param>
        /// <param name="exception">The exception thrown during guard evaluation.</param>
        /// <returns>A task yielding <see langword="true"/> to acknowledge the failure or <see langword="false"/> to leave it unacknowledged; neither value recovers execution.</returns>
        Task<bool> OnFlowGuardExceptionAsync<TToken>(IGuardContext<TToken> guardContext, Exception exception);

        /// <summary>Receives notification of a failure while transforming a flow token.</summary>
        /// <typeparam name="TToken">The input CLR token type accepted by the transformation.</typeparam>
        /// <typeparam name="TTransformedToken">The CLR token type the transformation was expected to produce.</typeparam>
        /// <param name="transformationContext">The context captured for the flow-token transformation that failed.</param>
        /// <param name="exception">The exception thrown during token transformation.</param>
        /// <returns>A task yielding <see langword="true"/> to acknowledge the failure or <see langword="false"/> to leave it unacknowledged; neither value recovers execution.</returns>
        Task<bool> OnFlowTransformationExceptionAsync<TToken, TTransformedToken>(ITransformationContext<TToken> transformationContext, Exception exception);
    }
}
