using System.Threading;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.Context
{
    public static class ActivitiesContextHolder
    {
        public static readonly AsyncLocal<IActivityContext> ActivityContext = new AsyncLocal<IActivityContext>();
        public static readonly AsyncLocal<INodeContext> NodeContext = new AsyncLocal<INodeContext>();
        public static readonly AsyncLocal<IFlowContext> FlowContext = new AsyncLocal<IFlowContext>();
        public static AsyncLocal<IExecutionContext> ExecutionContext => CommonContextHolder.ExecutionContext;
        public static readonly AsyncLocal<IActivityInspection> Inspection = new AsyncLocal<IActivityInspection>();
        public static readonly AsyncLocal<IExceptionContext> ExceptionContext = new AsyncLocal<IExceptionContext>();
    }
}
