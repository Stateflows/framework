using System.Threading;
using Stateflows.Common;
using Stateflows.Common.Context;

namespace Stateflows.StateMachines.Context
{
    public static class StateMachinesContextHolder
    {
        public static readonly AsyncLocal<IStateMachineContext> StateMachineContext = new AsyncLocal<IStateMachineContext>();
        public static readonly AsyncLocal<IStateContext> StateContext = new AsyncLocal<IStateContext>();
        public static readonly AsyncLocal<ITransitionContext> TransitionContext = new AsyncLocal<ITransitionContext>();
        public static AsyncLocal<IExecutionContext> ExecutionContext => CommonContextHolder.ExecutionContext;
    }
}
