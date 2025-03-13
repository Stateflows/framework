using System.Threading;
using Stateflows.Common.Context;

namespace Stateflows.StateMachines.Context
{
    public static class StateMachinesContextHolder
    {
        public static readonly AsyncLocal<IStateMachineContext> StateMachineContext = new AsyncLocal<IStateMachineContext>();
        public static readonly AsyncLocal<IStateContext> StateContext = new AsyncLocal<IStateContext>();
        public static readonly AsyncLocal<ITransitionContext> TransitionContext = new AsyncLocal<ITransitionContext>();
        public static readonly AsyncLocal<IExecutionContext> ExecutionContext = new AsyncLocal<IExecutionContext>();
        public static readonly AsyncLocal<IStateMachineInspection> Inspection = new AsyncLocal<IStateMachineInspection>();
        public static AsyncLocal<Common.IExecutionContext> CommonExecutionContext => CommonContextHolder.ExecutionContext;
    }
}
