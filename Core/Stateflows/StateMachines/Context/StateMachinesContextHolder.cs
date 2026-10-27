using System.Threading;
using Stateflows.Common.Context;

namespace Stateflows.StateMachines.Context
{
    /// <summary>Holds ambient execution contexts used to resolve callback services within the current asynchronous flow.</summary>
    /// <remarks>Applications should consume injected context interfaces rather than replace these runtime-managed values.</remarks>
    public static class StateMachinesContextHolder
    {
        /// <summary>The machine instance context associated with the current asynchronous flow.</summary>
        public static readonly AsyncLocal<IStateMachineContext> StateMachineContext = new ();
        /// <summary>The current state's context during a state-scoped callback.</summary>
        public static readonly AsyncLocal<IStateContext> StateContext = new ();
        /// <summary>The current transition context during guard or effect execution.</summary>
        public static readonly AsyncLocal<ITransitionContext> TransitionContext = new ();
        /// <summary>The current machine execution context and recorded transition steps.</summary>
        public static readonly AsyncLocal<IExecutionContext> ExecutionContext = new ();
        /// <summary>The runtime inspection model associated with the current machine execution.</summary>
        public static readonly AsyncLocal<IStateMachineInspection> Inspection = new ();
        /// <summary>The shared behavior execution context holder for the current asynchronous flow.</summary>
        public static AsyncLocal<Common.IExecutionContext> CommonExecutionContext => CommonContextHolder.ExecutionContext;
        /// <summary>The shared current-behavior context holder.</summary>
        public static AsyncLocal<Common.IBehaviorContext> BehaviorContext => CommonContextHolder.BehaviorContext;
        /// <summary>The shared enclosing behavior's context holder.</summary>
        public static AsyncLocal<Common.IParentBehaviorContext> ParentBehaviorContext => CommonContextHolder.ParentBehaviorContext;
        /// <summary>The shared owning behavior's context holder.</summary>
        public static AsyncLocal<Common.IOwnerBehaviorContext> OwnerBehaviorContext => CommonContextHolder.OwnerBehaviorContext;
    }
}
