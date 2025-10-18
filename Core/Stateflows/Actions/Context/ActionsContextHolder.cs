using System.Threading;
using Stateflows.Common;
using Stateflows.Common.Context;

namespace Stateflows.Actions.Context
{
    public static class ActionsContextHolder
    {
        public static AsyncLocal<IExecutionContext> ExecutionContext => CommonContextHolder.ExecutionContext;
        public static AsyncLocal<IBehaviorContext> BehaviorContext => CommonContextHolder.BehaviorContext;
        public static AsyncLocal<IActionContext> ActionContext = new ();
    }
}
