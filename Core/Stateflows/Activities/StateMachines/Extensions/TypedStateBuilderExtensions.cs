using Stateflows.Activities.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class TypedStateBuilderExtensions
    {
        public static ITypedStateBuilder AddOnEntryActivity(this ITypedStateBuilder builder, string activityName, StateActionActivityInitializationBuilder parametersBuilder = null, IntegratedActivityBuildAction buildAction = null)
            => (builder as IStateBuilder).AddOnEntryActivity(activityName, parametersBuilder, buildAction) as ITypedStateBuilder;

        public static ITypedStateBuilder AddOnEntryActivity<TActivity>(this ITypedStateBuilder builder, StateActionActivityInitializationBuilder parametersBuilder = null, IntegratedActivityBuildAction buildAction = null)
            where TActivity : Activity
            => (builder as IStateBuilder).AddOnEntryActivity<TActivity>(parametersBuilder, buildAction) as ITypedStateBuilder;

        public static ITypedStateBuilder AddOnExitActivity(this ITypedStateBuilder builder, string activityName, StateActionActivityInitializationBuilder parametersBuilder = null, IntegratedActivityBuildAction buildAction = null)
            => (builder as IStateBuilder).AddOnExitActivity(activityName, parametersBuilder, buildAction) as ITypedStateBuilder;

        public static ITypedStateBuilder AddOnExitActivity<TActivity>(this ITypedStateBuilder builder, StateActionActivityInitializationBuilder parametersBuilder = null, IntegratedActivityBuildAction buildAction = null)
            where TActivity : Activity
            => (builder as IStateBuilder).AddOnExitActivity<TActivity>(parametersBuilder, buildAction) as ITypedStateBuilder;
    }
}
