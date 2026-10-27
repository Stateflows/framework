using System.Threading;
using Stateflows.Common;
using Stateflows.Common.Context;

namespace Stateflows.Actions.Context
{
    /// <summary>Exposes action-related contexts for the current asynchronous execution flow.</summary>
    /// <remarks>Values are ambient and temporary; do not retain contexts beyond the operation in which they are supplied.</remarks>
    public static class ActionsContextHolder
    {
        /// <summary>The shared execution context currently active in this asynchronous flow.</summary>
        public static AsyncLocal<IExecutionContext> ExecutionContext => CommonContextHolder.ExecutionContext;

        /// <summary>The shared behavior context currently active in this asynchronous flow.</summary>
        public static AsyncLocal<IBehaviorContext> BehaviorContext => CommonContextHolder.BehaviorContext;

        /// <summary>The parent behavior context currently active in this asynchronous flow, if any.</summary>
        public static AsyncLocal<Common.IParentBehaviorContext> ParentBehaviorContext => CommonContextHolder.ParentBehaviorContext;

        /// <summary>The owner behavior context currently active in this asynchronous flow, if any.</summary>
        public static AsyncLocal<Common.IOwnerBehaviorContext> OwnerBehaviorContext => CommonContextHolder.OwnerBehaviorContext;

        /// <summary>The action context currently active in this asynchronous flow.</summary>
        public static AsyncLocal<IActionContext> ActionContext = new ();
    }
}
