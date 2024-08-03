using System.Threading;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context
{
    public static class ContextHolder
    {
        public static readonly AsyncLocal<IActivityContext> ActivityContext = new AsyncLocal<IActivityContext>();
        public static readonly AsyncLocal<INodeContext> NodeContext = new AsyncLocal<INodeContext>();
        public static readonly AsyncLocal<IFlowContext> FlowContext = new AsyncLocal<IFlowContext>();
        public static readonly AsyncLocal<IExecutionContext> ExecutionContext = new AsyncLocal<IExecutionContext>();
        public static readonly AsyncLocal<IExceptionContext> ExceptionContext = new AsyncLocal<IExceptionContext>();
    }
}
