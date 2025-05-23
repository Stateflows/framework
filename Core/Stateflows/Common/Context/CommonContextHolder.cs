using System.Threading;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Common.Context
{
    public static class CommonContextHolder
    {
        public static readonly AsyncLocal<IExecutionContext> ExecutionContext = new AsyncLocal<IExecutionContext>();
        public static readonly AsyncLocal<IBehaviorContext> BehaviorContext = new AsyncLocal<IBehaviorContext>();
    }
}
