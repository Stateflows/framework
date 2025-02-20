using Stateflows.Actions;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.Activities
{
    internal static class IStateActionContextExtensions
    {
        public static string GetBehaviorInstance(this IStateActionContext context, string action)
            => $"{context.StateMachine.Id}.{context.CurrentState.Name}.{action}";

        public static bool TryLocateActivity(this IStateActionContext context, string activityName, string action, out IActivityBehavior activity)
            => context.TryLocateActivity(new ActivityId(activityName, context.GetBehaviorInstance(action)), out activity);
        
        public static bool TryLocateAction(this IStateActionContext context, string activityName, string action, out IActionBehavior activity)
            => context.TryLocateAction(new ActionId(activityName, context.GetBehaviorInstance(action)), out activity);
    }
}
