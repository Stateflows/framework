using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.Activities
{
    internal static class IStateActionContextExtensions
    {
        public static string GetActivityInstance(this IStateActionContext context, string action)
            => $"{context.StateMachine.Id}.{context.CurrentState.Name}.{action}";

        public static bool TryLocateActivity(this IStateActionContext context, string activityName, string action, out IActivity activity)
            => context.TryLocateActivity(new ActivityId(activityName, context.GetActivityInstance(action)), out activity);
    }
}
