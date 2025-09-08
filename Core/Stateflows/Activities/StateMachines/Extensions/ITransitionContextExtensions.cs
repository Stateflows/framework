using System;
using Stateflows.Actions;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.Activities
{
    internal static class ITransitionContextExtensions
    {
        public static string GetBehaviorInstance<TEvent>(this ITransitionContext<TEvent> context, string action)
            => $"{context.Behavior.Id.Name}:{context.Source.Name}:{action}";

        public static bool TryLocateActivity<TEvent>(this ITransitionContext<TEvent> context, string activityName, string instance, out IActivityBehavior activity)
            => context.TryLocateActivity(new ActivityId(activityName, instance), out activity);

        public static bool TryLocateAction<TEvent>(this ITransitionContext<TEvent> context, string actionName, string instance, out IActionBehavior action)
            => context.TryLocateAction(new ActionId(actionName, instance), out action);
    }
}
