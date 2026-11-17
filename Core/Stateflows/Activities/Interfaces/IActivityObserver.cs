using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>Observes activity graph actions, lifecycle operations, and flow-pipeline evaluations.</summary>
    /// <remarks>
    /// Global observers wrap local observers, with each group applied in registration
    /// order. Each observer must invoke and await its continuation exactly once.
    /// Results from result-bearing continuations are exposed for inspection, not
    /// replacement: the pipeline returns the continuation's result even if an
    /// observer ignores it. Observers must not change activity state or tokens, or
    /// suppress continuation failures. Use <see langword="try"/>/<see langword="finally"/>
    /// for cleanup that must run on failure. Missing, repeated, or unfinished
    /// continuation calls fail execution, and an exception thrown by an observer
    /// also propagates as an operation failure.
    /// </remarks>
    public interface IActivityObserver
    {
        /// <summary>Observes one registered node action or lifecycle action within its existing aggregate scope.</summary>
        /// <param name="actionContext">The observation context identifying the activity action and its execution scope.</param>
        /// <param name="next">The continuation for remaining observers and the action; invoke and await it exactly once.</param>
        /// <returns>A task completed when observation and the action have completed.</returns>
        Task OnActionAsync(IActivityActionObservationContext actionContext, Func<Task> next);

        /// <summary>Observes an individual initialization predicate, flow predicate, or short-circuit Boolean group.</summary>
        /// <remarks>
        /// Runs only for registrations actually evaluated. A false initializer result skips later initializers;
        /// Boolean groups preserve their declared short-circuit behavior. The enclosing initialization or
        /// flow-guard callback observes the entire operation, not each constituent predicate.
        /// </remarks>
        /// <param name="predicateContext">The registration identity, owner, Boolean kind, and event; flow predicates also expose token metadata.</param>
        /// <param name="next">The remaining observers and predicate evaluation; invoke and await exactly once without replacing its result.</param>
        /// <returns>A task completed after predicate evaluation and observation; failures propagate unchanged.</returns>
        Task OnPredicateAsync(IActivityPredicateObservationContext predicateContext, Func<Task<bool>> next);

        /// <summary>Observes activity initialization before the operation and remaining observers.</summary>
        /// <remarks>
        /// Wraps the whole attempt, including initializer selection and any individual initializer
        /// <see cref="OnPredicateAsync"/> callbacks. It also runs when no initializer predicate exists.
        /// </remarks>
        /// <param name="initializationContext">The context identifying the activity initialization operation.</param>
        /// <param name="implicitInitialization">Whether the engine initiated initialization implicitly rather than through an explicit initialization request.</param>
        /// <param name="next">The continuation for remaining observers and initialization; invoke and await it exactly once.</param>
        /// <returns>A task completed when observation and initialization have completed; the initialization result is the continuation's result and cannot be replaced by an observer.</returns>
        Task OnActivityInitializeAsync(IActivityInitializationContext initializationContext, bool implicitInitialization, Func<Task<bool>> next);

        /// <summary>Observes activity finalization before the operation and remaining observers.</summary>
        /// <param name="finalizationContext">The context identifying the activity finalization operation.</param>
        /// <param name="next">The continuation for remaining observers and finalization; invoke and await it exactly once.</param>
        /// <returns>A task completed when observation and finalization have completed.</returns>
        Task OnActivityFinalizeAsync(IActivityFinalizationContext finalizationContext, Func<Task> next);

        /// <summary>Observes node initialization before the operation and remaining observers.</summary>
        /// <param name="nodeContext">The context identifying the node being initialized.</param>
        /// <param name="next">The continuation for remaining observers and node initialization; invoke and await it exactly once.</param>
        /// <returns>A task completed when observation and node initialization have completed.</returns>
        Task OnNodeInitializeAsync(IActivityNodeContext nodeContext, Func<Task> next);

        /// <summary>Observes node finalization before the operation and remaining observers.</summary>
        /// <param name="nodeContext">The context identifying the node being finalized.</param>
        /// <param name="next">The continuation for remaining observers and node finalization; invoke and await it exactly once.</param>
        /// <returns>A task completed when observation and node finalization have completed.</returns>
        Task OnNodeFinalizeAsync(IActivityNodeContext nodeContext, Func<Task> next);

        /// <summary>Observes the activation decision for a node.</summary>
        /// <param name="nodeContext">The context identifying the node whose activation was evaluated.</param>
        /// <param name="activated">Whether the node was activated by the evaluation that preceded this callback.</param>
        /// <param name="next">The continuation for remaining observers and subsequent activation processing; invoke and await it exactly once.</param>
        /// <returns>A task completed when observation and subsequent activation processing have completed.</returns>
        Task OnNodeActivateAsync(IActivityNodeContext nodeContext, bool activated, Func<Task> next);

        /// <summary>Observes execution of a node's registered action operations.</summary>
        /// <param name="nodeContext">The context identifying the node whose action is executing.</param>
        /// <param name="next">The continuation for remaining observers and node action execution; invoke and await it exactly once.</param>
        /// <returns>A task completed when observation and node action execution have completed.</returns>
        Task OnNodeExecuteAsync(IActivityNodeContext nodeContext, Func<Task> next);

        /// <summary>Observes flow activation before transfer evaluation.</summary>
        /// <param name="flowContext">The before-flow context identifying the flow and its source-side evaluation data.</param>
        /// <param name="next">The continuation for remaining observers and flow processing, yielding the after-flow context; invoke and await it exactly once.</param>
        /// <returns>A task completed when observation and flow processing have completed; the pipeline retains the continuation's after-flow context.</returns>
        Task OnFlowActivateAsync(IActivityBeforeFlowContext flowContext, Func<Task<IActivityAfterFlowContext>> next);

        /// <summary>Observes a typed flow-guard evaluation, including its constituent predicate or Boolean group.</summary>
        /// <typeparam name="TToken">The CLR token type considered by the guard.</typeparam>
        /// <param name="guardContext">The context identifying the flow guard and the token being considered.</param>
        /// <param name="next">The continuation for remaining observers and guard evaluation; invoke and await it exactly once.</param>
        /// <returns>A task completed when observation and guard evaluation have completed; the pipeline retains the continuation's Boolean result.</returns>
        Task OnFlowGuardAsync<TToken>(IGuardContext<TToken> guardContext, Func<Task<bool>> next);

        /// <summary>Observes transformation of a token while it passes through a flow.</summary>
        /// <typeparam name="TToken">The input CLR token type available in the transformation context.</typeparam>
        /// <typeparam name="TTransformedToken">The CLR token type returned by the transformation continuation.</typeparam>
        /// <param name="transformationContext">The flow context, transformation registration identity and order, and actual input token and type metadata.</param>
        /// <param name="next">The continuation for remaining observers and transformation execution; invoke and await it exactly once.</param>
        /// <returns>A task completed when observation and transformation execution have completed; the pipeline retains the continuation's transformed token.</returns>
        Task OnFlowTransformAsync<TToken, TTransformedToken>(ITransformationContext<TToken> transformationContext, Func<Task<TTransformedToken>> next);
    }
}
