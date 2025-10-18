using System.Threading;
using Stateflows.Common.Context;

namespace Stateflows.StateMachines.Context
{
    public static class StateMachinesContextHolder
    {
        public static readonly AsyncLocal<IStateMachineContext> StateMachineContext = new ();
        public static readonly AsyncLocal<IStateContext> StateContext = new ();
        public static readonly AsyncLocal<ITransitionContext> TransitionContext = new ();
        public static readonly AsyncLocal<IExecutionContext> ExecutionContext = new ();
        public static readonly AsyncLocal<IStateMachineInspection> Inspection = new ();
        public static AsyncLocal<Common.IExecutionContext> CommonExecutionContext => CommonContextHolder.ExecutionContext;
        public static AsyncLocal<Common.IBehaviorContext> BehaviorContext => CommonContextHolder.BehaviorContext;
    }
}
