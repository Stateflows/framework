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
        Task OnActivityInitializeAsync(IActivityInitializationContext context, bool implicitInitialization, Func<Task<bool>> next);

        Task OnActivityFinalizeAsync(IActivityFinalizationContext context, Func<Task> next);

        Task OnNodeInitializeAsync(IActivityNodeContext context, Func<Task> next);

        Task OnNodeFinalizeAsync(IActivityNodeContext context, Func<Task> next);

        Task OnNodeActivateAsync(IActivityNodeContext context, bool activated, Func<Task> next);

        Task OnNodeExecuteAsync(IActivityNodeContext context, Func<Task> next);

        Task OnFlowActivateAsync(IActivityBeforeFlowContext context, Func<Task<IActivityAfterFlowContext>> next);

        Task OnFlowGuardAsync<TToken>(IGuardContext<TToken> context, Func<Task<bool>> next);

        Task OnFlowTransformAsync<TToken, TTransformedToken>(ITransformationContext<TToken> context, Func<Task<TTransformedToken>> next);
    }
}
