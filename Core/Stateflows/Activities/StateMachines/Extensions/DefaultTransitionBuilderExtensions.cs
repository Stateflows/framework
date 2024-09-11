using System.Diagnostics;
using Stateflows.Activities.Extensions;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class DefaultTransitionBuilderExtensions
    {
        #region AddGuardActivity
        [DebuggerHidden]
        public static IDefaultTransitionBuilder AddGuardActivity(this IDefaultTransitionBuilder builder, string activityName, TransitionActivityBuildAction<CompletionEvent> buildAction = null)
            => builder.AddGuard(c => StateMachineActivityExtensions.RunGuardActivity(c, activityName, buildAction));

        [DebuggerHidden]
        public static IDefaultTransitionBuilder AddGuardActivity<TEvent, TActivity>(this IDefaultTransitionBuilder builder, TransitionActivityBuildAction<CompletionEvent> buildAction = null)
            where TActivity : class, IActivity
            => builder.AddGuardActivity(Activity<TActivity>.Name, buildAction);
        #endregion

        #region AddEffectActivity
        [DebuggerHidden]
        public static IDefaultTransitionBuilder AddEffectActivity(this IDefaultTransitionBuilder builder, string activityName, TransitionActivityBuildAction<CompletionEvent> buildAction = null)
            => builder.AddEffect(c => StateMachineActivityExtensions.RunEffectActivity(c, activityName, buildAction));

        [DebuggerHidden]
        public static IDefaultTransitionBuilder AddEffectActivity<TActivity>(this IDefaultTransitionBuilder builder, TransitionActivityBuildAction<CompletionEvent> buildAction = null)
            where TActivity : class, IActivity
            => builder.AddEffectActivity(Activity<TActivity>.Name, buildAction);
        #endregion
    }
}