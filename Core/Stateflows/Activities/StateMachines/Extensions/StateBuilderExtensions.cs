using System.Diagnostics;
using Stateflows.StateMachines.Sync;
using Stateflows.Activities.Extensions;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class StateBuilderExtensions
    {
        [DebuggerHidden]
        public static IStateBuilder AddOnEntryActivity(this IStateBuilder builder, string activityName, StateActionActivityBuildAction buildAction = null)
            => builder.AddOnEntry(c => StateMachineActivityExtensions.RunStateActivity(Constants.Entry, c, activityName, buildAction));

        [DebuggerHidden]
        public static IStateBuilder AddOnEntryActivity<TActivity>(this IStateBuilder builder, StateActionActivityBuildAction buildAction = null)
            where TActivity : class, IActivity
            => AddOnEntryActivity(builder, Activity<TActivity>.Name, buildAction);

        [DebuggerHidden]
        public static IStateBuilder AddOnExitActivity(this IStateBuilder builder, string activityName, StateActionActivityBuildAction buildAction = null)
            => builder.AddOnExit(c => StateMachineActivityExtensions.RunStateActivity(Constants.Exit, c, activityName, buildAction));

        [DebuggerHidden]
        public static IStateBuilder AddOnExitActivity<TActivity>(this IStateBuilder builder, StateActionActivityBuildAction buildAction = null)
            where TActivity : class, IActivity
            => AddOnExitActivity(builder, Activity<TActivity>.Name, buildAction);
    }
}
