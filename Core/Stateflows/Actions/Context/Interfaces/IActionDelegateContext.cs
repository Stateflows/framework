using System.Threading;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Actions.Context.Interfaces
{
    /// <summary>Execution context supplied to an action delegate, with behavior identity, input/output, and cancellation.</summary>
    /// <remarks>Cancellation tokens should be observed by asynchronous work so event processing can stop promptly.</remarks>
    public interface IActionDelegateContext : IBehaviorLocator, IExecutionContext, IInput, IOutput
    {
        /// <summary>
        /// Information about current behavior
        /// </summary>
        IBehaviorContext Behavior { get; }

        /// <summary>Gets the parent behavior context when this behavior is nested.</summary>
        /// <param name="parentBehaviorContext">Receives the parent context when one is available.</param>
        /// <returns><see langword="true"/> if a parent context exists; otherwise, <see langword="false"/>.</returns>
        bool TryGetParentBehaviorContext(out IParentBehaviorContext parentBehaviorContext);

        /// <summary>Gets the owner behavior context when this behavior is owned by another behavior.</summary>
        /// <param name="ownerBehaviorContext">Receives the owner context when one is available.</param>
        /// <returns><see langword="true"/> if an owner context exists; otherwise, <see langword="false"/>.</returns>
        bool TryGetOwnerBehaviorContext(out IOwnerBehaviorContext ownerBehaviorContext);

        /// <summary>
        /// Cancellation token for interrupting this action's execution.
        /// </summary>
        CancellationToken CancellationToken { get; }
    }
}
