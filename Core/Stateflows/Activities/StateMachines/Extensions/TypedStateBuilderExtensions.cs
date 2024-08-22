using Stateflows.Activities.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class TypedStateBuilderExtensions
    {
        public static ITypedStateBuilder AddOnEntryActivity(this ITypedStateBuilder builder, string activityName, StateActionActivityBuildAction buildAction = null)
            => (builder as IStateBuilder).AddOnEntryActivity(activityName, buildAction) as ITypedStateBuilder;

        public static ITypedStateBuilder AddOnEntryActivity<TActivity>(this ITypedStateBuilder builder, StateActionActivityBuildAction buildAction = null)
            where TActivity : class, IActivity
            => (builder as IStateBuilder).AddOnEntryActivity<TActivity>(buildAction) as ITypedStateBuilder;

        public static ITypedStateBuilder AddOnExitActivity(this ITypedStateBuilder builder, string activityName, StateActionActivityBuildAction buildAction = null)
            => (builder as IStateBuilder).AddOnExitActivity(activityName, buildAction) as ITypedStateBuilder;

        public static ITypedStateBuilder AddOnExitActivity<TActivity>(this ITypedStateBuilder builder, StateActionActivityBuildAction buildAction = null)
            where TActivity : class, IActivity
            => (builder as IStateBuilder).AddOnExitActivity<TActivity>(buildAction) as ITypedStateBuilder;
    }
}
