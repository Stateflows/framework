using Stateflows.Actions;
using Stateflows.StateMachines;

namespace Stateflows.Activities
{
    internal static class IStateActionContextExtensions
    {
        public static string GetBehaviorInstance(this IStateActionContext context, string action)
            => $"{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}:{context.State.Name}:{action}";

        public static bool TryLocateActivity(this IStateActionContext context, string actionName, string action, out IActivityBehavior activity)
            => context.TryLocateActivity(new ActivityId(actionName, context.GetBehaviorInstance(action)), out activity);
        
        public static bool TryLocateAction(this IStateActionContext context, string actionName, string action, out IActionBehavior activity)
            => context.TryLocateAction(new ActionId(actionName, context.GetBehaviorInstance(action)), out activity);
    }
}
