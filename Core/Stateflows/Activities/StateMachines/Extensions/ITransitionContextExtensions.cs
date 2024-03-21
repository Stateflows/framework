using System;
using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.Activities
{
    internal static class ITransitionContextExtensions
    {
        public static string GetDoActivityInstance<TEvent>(this ITransitionContext<TEvent> context)
            where TEvent : Event, new()
            => $"{context.StateMachine.Id}.{context.SourceState.Name}.{Constants.Do}";

        public static bool TryLocateDoActivity<TEvent>(this ITransitionContext<TEvent> context, string activityName, out IActivity activity)
            where TEvent : Event, new()
            => context.TryLocateActivity(new ActivityId(activityName, context.GetDoActivityInstance()), out activity);

        public static string GetActivityInstance<TEvent>(this ITransitionContext<TEvent> context, string action)
            where TEvent : Event, new()
            => $"{context.StateMachine.Id}.{context.SourceState.Name}.{action}.{new Random().Next()}";

        public static bool TryLocateActivity<TEvent>(this ITransitionContext<TEvent> context, string activityName, string action, out IActivity activity)
            where TEvent : Event, new()
            => context.TryLocateActivity(new ActivityId(activityName, context.GetActivityInstance(action)), out activity);
    }
}
