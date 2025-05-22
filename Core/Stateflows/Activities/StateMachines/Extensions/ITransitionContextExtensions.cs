using System;
using Stateflows.Actions;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Registration;

namespace Stateflows.Activities
{
    internal static class ITransitionContextExtensions
    {
        private static string GetDoActivityInstance<TEvent>(this ITransitionContext<TEvent> context)
            => $"{context.Behavior.Id}.{context.Source.Name}.{Constants.Do}";

        public static bool TryLocateDoActivity<TEvent>(this ITransitionContext<TEvent> context, string activityName, out IActivityBehavior activity)
            => context.TryLocateActivity(new ActivityId(activityName, context.GetDoActivityInstance()), out activity);

        private static string GetBehaviorInstance<TEvent>(this ITransitionContext<TEvent> context, string action)
            => $"{context.Behavior.Id}.{context.Source.Name}.{action}.{new Random().Next()}";

        public static bool TryLocateActivity<TEvent>(this ITransitionContext<TEvent> context, string activityName, string stateAction, out IActivityBehavior activity)
            => context.TryLocateActivity(new ActivityId(activityName, context.GetBehaviorInstance(stateAction)), out activity);

        public static bool TryLocateAction<TEvent>(this ITransitionContext<TEvent> context, string actionName, string stateAction, out IActionBehavior action)
            => context.TryLocateAction(new ActionId(actionName, context.GetBehaviorInstance(stateAction)), out action);
    }
}
