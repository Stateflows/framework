using System.Diagnostics;
using Stateflows.Activities.Extensions;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class CompositeStateBuilderExtensions
    {
        // [DebuggerHidden]
        // public static ICompositeStateBuilder AddOnEntryActivity(this ICompositeStateBuilder builder, string activityName, StateActionActivityBuildAction buildAction = null)
        //     => builder.AddOnEntry(c => StateMachineActivityExtensions.RunStateActivity(Constants.Entry, c, activityName, buildAction));
        //
        // [DebuggerHidden]
        // public static ICompositeStateBuilder AddOnEntryActivity<TActivity>(this ICompositeStateBuilder builder, StateActionActivityBuildAction buildAction = null)
        //     where TActivity : class, IActivity
        //     => AddOnEntryActivity(builder, Activity<TActivity>.Name, buildAction);
        //
        // [DebuggerHidden]
        // public static ICompositeStateBuilder AddOnExitActivity(this ICompositeStateBuilder builder, string activityName, StateActionActivityBuildAction buildAction = null)
        //     => builder.AddOnExit(c => StateMachineActivityExtensions.RunStateActivity(Constants.Exit, c, activityName, buildAction));
        //
        // [DebuggerHidden]
        // public static ICompositeStateBuilder AddOnExitActivity<TActivity>(this ICompositeStateBuilder builder, StateActionActivityBuildAction buildAction = null)
        //     where TActivity : class, IActivity
        //     => AddOnExitActivity(builder, Activity<TActivity>.Name, buildAction);
    }
}
