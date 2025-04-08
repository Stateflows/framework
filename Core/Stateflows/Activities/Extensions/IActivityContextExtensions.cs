using Stateflows.Activities.Engine;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities.Extensions
{
    internal static class IActivityContextExtensions
    {
        public static Executor GetExecutor(this IActivityContext context)
            => ((ActivityContext)context).Context.Executor;

        public static Node GetNode(this IActionContext context)
            => ((ActionContext)context).Node;
        
        public static Executor GetExecutor(this IBehaviorContext context)
            => ((ActivityContext)context).Context.Executor;
        
        public static RootContext GetContext(this IBehaviorContext context)
            => ((BaseContext)context).Context;

        public static NodeScope GetNodeScope(this IBehaviorContext context)
            => ((BaseContext)context).NodeScope;
    }
}
