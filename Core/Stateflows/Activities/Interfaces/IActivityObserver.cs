using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>
    /// Observes activity operations. Global observers wrap local observers,
    /// in registration order within each group.
    /// </summary>
    /// <remarks>
    /// Call and await next exactly once. Results are exposed for inspection, not
    /// replacement. Observers must not change state or tokens, or suppress continuation
    /// failures. Use try/finally for cleanup that must run on failure. Missing, repeated,
    /// or unfinished continuation calls fail execution.
    /// </remarks>
    public interface IActivityObserver
    {
        /// <summary>Observes activity initialization and may continue or replace its result.</summary>
        /// <param name="context">The activity initialization context.</param>
        /// <param name="implicitInitialization">Whether initialization was triggered implicitly.</param>
        /// <param name="next">The continuation for the remaining observers and initialization operation.</param>
        Task OnActivityInitializeAsync(IActivityInitializationContext context, bool implicitInitialization, Func<Task<bool>> next);

        /// <summary>Observes activity finalization.</summary>
        /// <param name="context">The activity finalization context.</param>
        /// <param name="next">The continuation for the remaining observers and finalization operation.</param>
        Task OnActivityFinalizeAsync(IActivityFinalizationContext context, Func<Task> next);

        /// <summary>Observes node initialization.</summary>
        /// <param name="context">The context of the node being initialized.</param>
        /// <param name="next">The continuation for the remaining observers and initialization operation.</param>
        Task OnNodeInitializeAsync(IActivityNodeContext context, Func<Task> next);

        /// <summary>Observes node finalization.</summary>
        /// <param name="context">The context of the node being finalized.</param>
        /// <param name="next">The continuation for the remaining observers and finalization operation.</param>
        Task OnNodeFinalizeAsync(IActivityNodeContext context, Func<Task> next);

        /// <summary>Observes the result of node activation evaluation.</summary>
        /// <param name="context">The context of the node being evaluated.</param>
        /// <param name="activated">Whether the node activated.</param>
        /// <param name="next">The continuation for the remaining observers and activation operation.</param>
        Task OnNodeActivateAsync(IActivityNodeContext context, bool activated, Func<Task> next);

        /// <summary>Observes node action execution.</summary>
        /// <param name="context">The context of the node action being executed.</param>
        /// <param name="next">The continuation for the remaining observers and action operation.</param>
        Task OnNodeExecuteAsync(IActivityNodeContext context, Func<Task> next);

        /// <summary>Observes flow activation before transfer and receives its resulting flow context after continuation.</summary>
        /// <param name="context">The flow context before transfer evaluation.</param>
        /// <param name="next">The continuation producing the post-evaluation flow context.</param>
        Task OnFlowActivateAsync(IActivityBeforeFlowContext context, Func<Task<IActivityAfterFlowContext>> next);

        /// <summary>Observes evaluation of a typed flow guard.</summary>
        /// <typeparam name="TToken">The type of token considered by the guard.</typeparam>
        /// <param name="context">The guard context.</param>
        /// <param name="next">The continuation producing the guard result.</param>
        Task OnFlowGuardAsync<TToken>(IGuardContext<TToken> context, Func<Task<bool>> next);

        /// <summary>Observes transformation of a token on a flow.</summary>
        /// <typeparam name="TToken">The input token type.</typeparam>
        /// <typeparam name="TTransformedToken">The transformed token type.</typeparam>
        /// <param name="context">The transformation context.</param>
        /// <param name="next">The continuation producing the transformed token.</param>
        Task OnFlowTransformAsync<TToken, TTransformedToken>(ITransformationContext<TToken> context, Func<Task<TTransformedToken>> next);
    }
}
