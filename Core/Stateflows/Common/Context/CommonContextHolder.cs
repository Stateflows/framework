using System.Threading;

namespace Stateflows.Common.Context
{
    public static class CommonContextHolder
    {
        public static readonly AsyncLocal<IExecutionContext> ExecutionContext = new AsyncLocal<IExecutionContext>();
        public static readonly AsyncLocal<IBehaviorContext> BehaviorContext = new AsyncLocal<IBehaviorContext>();
        public static readonly AsyncLocal<IParentBehaviorContext> ParentBehaviorContext = new AsyncLocal<IParentBehaviorContext>();
        public static readonly AsyncLocal<IOwnerBehaviorContext> OwnerBehaviorContext = new AsyncLocal<IOwnerBehaviorContext>();
    }
}
