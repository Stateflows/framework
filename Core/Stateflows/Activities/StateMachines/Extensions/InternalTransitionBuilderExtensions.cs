using System.Diagnostics;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class InternalTransitionBuilderExtensions
    {
        #region AddGuardActivity
        [DebuggerHidden]
        public static IInternalTransitionBuilder<TEvent> AddGuardActivity<TEvent>(this IInternalTransitionBuilder<TEvent> builder, string activityName, TransitionActivityBuildAction<TEvent> buildAction = null)
            where TEvent : Event, new()
            => builder.AddGuard(c => StateMachineActivityExtensions.RunGuardActivity(c, activityName, buildAction));

        [DebuggerHidden]
        public static IInternalTransitionBuilder<TEvent> AddGuardActivity<TEvent, TActivity>(this IInternalTransitionBuilder<TEvent> builder, TransitionActivityBuildAction<TEvent> buildAction = null)
            where TEvent : Event, new()
            where TActivity : class, IActivity
            => builder.AddGuardActivity(Activity<TActivity>.Name, buildAction);
        #endregion

        #region AddEffectActivity
        [DebuggerHidden]
        public static IInternalTransitionBuilder<TEvent> AddEffectActivity<TEvent>(this IInternalTransitionBuilder<TEvent> builder, string activityName, TransitionActivityBuildAction<TEvent> buildAction = null)
            where TEvent : Event, new()
            => builder.AddEffect(c => StateMachineActivityExtensions.RunEffectActivity(c, activityName, buildAction));

        [DebuggerHidden]
        public static IInternalTransitionBuilder<TEvent> AddEffectActivity<TEvent, TActivity>(this IInternalTransitionBuilder<TEvent> builder, TransitionActivityBuildAction<TEvent> buildAction = null)
            where TEvent : Event, new()
            where TActivity : class, IActivity
            => builder.AddEffectActivity(Activity<TActivity>.Name, buildAction);
        #endregion
    }
}