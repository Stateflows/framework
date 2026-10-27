using System.Threading;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.Context
{
    /// <summary>Exposes activity-related contexts for the current asynchronous execution flow.</summary>
    /// <remarks>Values are ambient and temporary; do not retain contexts beyond the operation in which they are supplied.</remarks>
    public static class ActivitiesContextHolder
    {
        /// <summary>The activity instance context currently active in this asynchronous flow.</summary>
        public static readonly AsyncLocal<IActivityContext> ActivityContext = new();

        /// <summary>The activity node context currently active in this asynchronous flow.</summary>
        public static readonly AsyncLocal<INodeContext> NodeContext = new();

        /// <summary>The activity flow context currently active in this asynchronous flow.</summary>
        public static readonly AsyncLocal<IFlowContext> FlowContext = new();

        /// <summary>The shared execution context currently active in this asynchronous flow.</summary>
        public static AsyncLocal<IExecutionContext> ExecutionContext => CommonContextHolder.ExecutionContext;

        /// <summary>The shared behavior context currently active in this asynchronous flow.</summary>
        public static AsyncLocal<IBehaviorContext> BehaviorContext => CommonContextHolder.BehaviorContext;

        /// <summary>The parent behavior context currently active in this asynchronous flow, if any.</summary>
        public static AsyncLocal<Common.IParentBehaviorContext> ParentBehaviorContext => CommonContextHolder.ParentBehaviorContext;

        /// <summary>The owner behavior context currently active in this asynchronous flow, if any.</summary>
        public static AsyncLocal<Common.IOwnerBehaviorContext> OwnerBehaviorContext => CommonContextHolder.OwnerBehaviorContext;

        /// <summary>The activity inspection snapshot currently active in this asynchronous flow.</summary>
        public static readonly AsyncLocal<IActivityInspection> Inspection = new();

        /// <summary>The activity exception context currently active in this asynchronous flow.</summary>
        public static readonly AsyncLocal<IExceptionContext> ExceptionContext = new();
    }
}
