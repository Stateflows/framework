using System.Threading;
using Stateflows.Common;
using Stateflows.Common.Context;

namespace Stateflows.Actions.Context
{
    public static class ActionsContextHolder
    {
        public static readonly AsyncLocal<IActionContext> ActionContext = new AsyncLocal<IActionContext>();
        public static AsyncLocal<IExecutionContext> ExecutionContext => CommonContextHolder.ExecutionContext;
        public static AsyncLocal<IBehaviorContext> BehaviorContext => CommonContextHolder.BehaviorContext;
    }
}
