using System.Diagnostics;
using Stateflows.Actions;
using Stateflows.Activities.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class TransitionBuilderExtensions
    {
        #region AddGuardBehavior
        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddGuardActivity<TEvent>(this ITransitionBuilder<TEvent> builder, string activityName, TransitionActivityBuildAction<TEvent> buildAction = null)
            => builder.AddGuard(c => StateMachineActivityExtensions.RunGuardActivity(c, activityName, buildAction));

        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddGuardActivity<TEvent, TActivity>(this ITransitionBuilder<TEvent> builder, TransitionActivityBuildAction<TEvent> buildAction = null)
            where TActivity : class, IActivity
            => builder.AddGuardActivity(Activity<TActivity>.Name, buildAction);
        
        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddGuardAction<TEvent>(this ITransitionBuilder<TEvent> builder, string actionName, TransitionActionBuildAction<TEvent> buildAction = null)
            => builder.AddGuard(c => StateMachineActionExtensions.RunGuardAction(c, actionName, buildAction));

        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddGuardAction<TEvent, TAction>(this ITransitionBuilder<TEvent> builder, TransitionActionBuildAction<TEvent> buildAction = null)
            where TAction : class, IAction
            => builder.AddGuardAction(Action<TAction>.Name, buildAction);
        #endregion

        #region AddEffectBehavior
        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddEffectActivity<TEvent>(this ITransitionBuilder<TEvent> builder, string activityName, TransitionActivityBuildAction<TEvent> buildAction = null)
            => builder.AddEffect(c => StateMachineActivityExtensions.RunEffectActivity(c, activityName, buildAction));

        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddEffectActivity<TEvent, TActivity>(this ITransitionBuilder<TEvent> builder, TransitionActivityBuildAction<TEvent> buildAction = null)
            where TActivity : class, IActivity
            => builder.AddEffectActivity(Activity<TActivity>.Name, buildAction);
        
        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddEffectAction<TEvent>(this ITransitionBuilder<TEvent> builder, string actionName, TransitionActionBuildAction<TEvent> buildAction = null)
            => builder.AddEffect(c => StateMachineActionExtensions.RunEffectAction(c, actionName, buildAction));

        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddEffectAction<TEvent, TAction>(this ITransitionBuilder<TEvent> builder, TransitionActionBuildAction<TEvent> buildAction = null)
            where TAction : class, IAction
            => builder.AddEffectAction(Action<TAction>.Name, buildAction);
        #endregion
    }
}