using Stateflows.Actions;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.Activities
{
    internal static class ITransitionContextExtensions
    {
        internal static bool TryLocateActivity<TEvent>(this ITransitionContext<TEvent> context, string activityName, string instance, out IActivityBehavior activity)
            => context.TryLocateActivity(new ActivityId(activityName, instance), out activity);

        internal static bool TryLocateAction<TEvent>(this ITransitionContext<TEvent> context, string actionName, string instance, out IActionBehavior action)
            => context.TryLocateAction(new ActionId(actionName, instance), out action);
        
        internal static bool TryLocateActivity<TEvent>(this IDeferralContext<TEvent> context, string activityName, string instance, out IActivityBehavior activity)
            => context.TryLocateActivity(new ActivityId(activityName, instance), out activity);

        internal static bool TryLocateAction<TEvent>(this IDeferralContext<TEvent> context, string actionName, string instance, out IActionBehavior action)
            => context.TryLocateAction(new ActionId(actionName, instance), out action);
    }
}
