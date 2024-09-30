using System;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.Activities
{
    internal static class ITransitionContextExtensions
    {
        public static string GetDoActivityInstance<TEvent>(this ITransitionContext<TEvent> context)
            => $"{context.StateMachine.Id}.{context.SourceState.Name}.{Constants.Do}";

        public static bool TryLocateDoActivity<TEvent>(this ITransitionContext<TEvent> context, string activityName, out IActivityBehavior activity)
            => context.TryLocateActivity(new ActivityId(activityName, context.GetDoActivityInstance()), out activity);

        public static string GetActivityInstance<TEvent>(this ITransitionContext<TEvent> context, string action)
            => $"{context.StateMachine.Id}.{context.SourceState.Name}.{action}.{new Random().Next()}";

        public static bool TryLocateActivity<TEvent>(this ITransitionContext<TEvent> context, string activityName, string action, out IActivityBehavior activity)
            => context.TryLocateActivity(new ActivityId(activityName, context.GetActivityInstance(action)), out activity);
    }
}
