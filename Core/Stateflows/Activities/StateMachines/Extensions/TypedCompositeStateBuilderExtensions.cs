using Stateflows.Activities.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class TypedCompositeStateBuilderExtensions
    {
        public static ITypedCompositeStateBuilder AddOnEntryActivity(this ITypedCompositeStateBuilder builder, string activityName, StateActionActivityInitializationBuilder parametersBuilder = null, IntegratedActivityBuildAction buildAction = null)
            => (builder as ICompositeStateBuilder).AddOnEntryActivity(activityName, parametersBuilder, buildAction) as ITypedCompositeStateBuilder;

        public static ITypedCompositeStateBuilder AddOnEntryActivity<TActivity>(this ITypedCompositeStateBuilder builder, StateActionActivityInitializationBuilder parametersBuilder = null, IntegratedActivityBuildAction buildAction = null)
            where TActivity : class, IActivity
            => (builder as ICompositeStateBuilder).AddOnEntryActivity<TActivity>(parametersBuilder, buildAction) as ITypedCompositeStateBuilder;

        public static ITypedCompositeStateBuilder AddOnExitActivity(this ITypedCompositeStateBuilder builder, string activityName, StateActionActivityInitializationBuilder parametersBuilder = null, IntegratedActivityBuildAction buildAction = null)
            => (builder as ICompositeStateBuilder).AddOnExitActivity(activityName, parametersBuilder, buildAction) as ITypedCompositeStateBuilder;

        public static ITypedCompositeStateBuilder AddOnExitActivity<TActivity>(this ITypedCompositeStateBuilder builder, StateActionActivityInitializationBuilder parametersBuilder = null, IntegratedActivityBuildAction buildAction = null)
            where TActivity : class, IActivity
            => (builder as ICompositeStateBuilder).AddOnExitActivity<TActivity>(parametersBuilder, buildAction) as ITypedCompositeStateBuilder;
    }
}
