using System.Threading;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Context
{
    public static class ContextHolder
    {
        public static readonly AsyncLocal<IStateMachineContext> StateMachineContext = new AsyncLocal<IStateMachineContext>();
        public static readonly AsyncLocal<IStateContext> StateContext = new AsyncLocal<IStateContext>();
        public static readonly AsyncLocal<ITransitionContext> TransitionContext = new AsyncLocal<ITransitionContext>();
        public static readonly AsyncLocal<IExecutionContext> ExecutionContext = new AsyncLocal<IExecutionContext>();
    }
}
