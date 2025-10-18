using Stateflows.Actions;
using Stateflows.StateMachines;

namespace Stateflows.Activities
{
    internal static class IStateActionContextExtensions
    {
        internal static bool TryLocateActivity(this IStateActionContext context, string actionName, string instance, out IActivityBehavior activity)
            => context.TryLocateActivity(new ActivityId(actionName, instance), out activity);
        
        internal static bool TryLocateAction(this IStateActionContext context, string actionName, string instance, out IActionBehavior activity)
            => context.TryLocateAction(new ActionId(actionName, instance), out activity);
    }
}
