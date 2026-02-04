using System.Threading;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.Context
{
    public static class ActivitiesContextHolder
    {
        public static readonly AsyncLocal<IActivityContext> ActivityContext = new();
        public static readonly AsyncLocal<INodeContext> NodeContext = new();
        public static readonly AsyncLocal<IFlowContext> FlowContext = new();
        public static AsyncLocal<IExecutionContext> ExecutionContext => CommonContextHolder.ExecutionContext;
        public static AsyncLocal<IBehaviorContext> BehaviorContext => CommonContextHolder.BehaviorContext;
        public static readonly AsyncLocal<IActivityInspection> Inspection = new();
        public static readonly AsyncLocal<IExceptionContext> ExceptionContext = new();
    }
}
