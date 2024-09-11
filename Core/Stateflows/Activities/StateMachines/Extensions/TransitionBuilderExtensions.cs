using System.Diagnostics;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class TransitionBuilderExtensions
    {
        #region AddGuardActivity
        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddGuardActivity<TEvent>(this ITransitionBuilder<TEvent> builder, string activityName, TransitionActivityBuildAction<TEvent> buildAction = null)
            where TEvent : Event, new()
            => builder.AddGuard(c => StateMachineActivityExtensions.RunGuardActivity(c, activityName, buildAction));

        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddGuardActivity<TEvent, TActivity>(this ITransitionBuilder<TEvent> builder, TransitionActivityBuildAction<TEvent> buildAction = null)
            where TEvent : Event, new()
            where TActivity : class, IActivity
            => builder.AddGuardActivity(Activity<TActivity>.Name, buildAction);
        #endregion

        #region AddEffectActivity
        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddEffectActivity<TEvent>(this ITransitionBuilder<TEvent> builder, string activityName, TransitionActivityBuildAction<TEvent> buildAction = null)
            where TEvent : Event, new()
            => builder.AddEffect(c => StateMachineActivityExtensions.RunEffectActivity(c, activityName, buildAction));

        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddEffectActivity<TEvent, TActivity>(this ITransitionBuilder<TEvent> builder, TransitionActivityBuildAction<TEvent> buildAction = null)
            where TEvent : Event, new()
            where TActivity : class, IActivity
            => builder.AddEffectActivity(Activity<TActivity>.Name, buildAction);
        #endregion
    }
}