using System.Diagnostics;
using Stateflows.Activities.Extensions;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class DefaultTransitionEFfectBuilderExtensions
    {
        #region AddEffectActivity
        [DebuggerHidden]
        public static IDefaultTransitionEffectBuilder AddEffectActivity(this IDefaultTransitionEffectBuilder builder, string activityName, TransitionActivityBuildAction<Completion> buildAction = null)
            => builder.AddEffect(c => StateMachineActivityExtensions.RunEffectActivity(c, activityName, buildAction));

        [DebuggerHidden]
        public static IDefaultTransitionEffectBuilder AddEffectActivity<TActivity>(this IDefaultTransitionEffectBuilder builder, TransitionActivityBuildAction<Completion> buildAction = null)
           where TActivity : class, IActivity
           => builder.AddEffectActivity(Activity<TActivity>.Name, buildAction);
        #endregion
    }
}