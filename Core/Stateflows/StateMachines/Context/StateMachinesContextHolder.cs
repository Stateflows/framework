using System.Threading;
using Stateflows.Common.Context;

namespace Stateflows.StateMachines.Context
{
    public static class StateMachinesContextHolder
    {
        public static readonly AsyncLocal<IStateMachineContext> StateMachineContext = new AsyncLocal<IStateMachineContext>();
        public static readonly AsyncLocal<IVertexContext> StateContext = new AsyncLocal<IVertexContext>();
        public static readonly AsyncLocal<ITransitionContext> TransitionContext = new AsyncLocal<ITransitionContext>();
        public static readonly AsyncLocal<IExecutionContext> ExecutionContext = new AsyncLocal<IExecutionContext>();
        public static AsyncLocal<Common.IExecutionContext> CommonExecutionContext => CommonContextHolder.ExecutionContext;
    }
}
