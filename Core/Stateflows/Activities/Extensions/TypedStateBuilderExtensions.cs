using Stateflows.Activities.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class TypedStateBuilderExtensions
    {
        public static ITypedStateBuilder AddOnEntryActivity(this ITypedStateBuilder builder, string activityName, StateActionActivityInitializationBuilder parametersBuilder = null)
            => (builder as IStateBuilder).AddOnEntryActivity(activityName, parametersBuilder) as ITypedStateBuilder;

        public static ITypedStateBuilder AddOnEntryActivity<TActivity>(this ITypedStateBuilder builder, StateActionActivityInitializationBuilder parametersBuilder = null)
            where TActivity : Activity
            => (builder as IStateBuilder).AddOnEntryActivity<TActivity>(parametersBuilder) as ITypedStateBuilder;

        public static ITypedStateBuilder AddOnExitActivity(this ITypedStateBuilder builder, string activityName, StateActionActivityInitializationBuilder parametersBuilder = null)
            => (builder as IStateBuilder).AddOnExitActivity(activityName, parametersBuilder) as ITypedStateBuilder;

        public static ITypedStateBuilder AddOnExitActivity<TActivity>(this ITypedStateBuilder builder, StateActionActivityInitializationBuilder parametersBuilder = null)
            where TActivity : Activity
            => (builder as IStateBuilder).AddOnExitActivity<TActivity>(parametersBuilder) as ITypedStateBuilder;

        public static ITypedStateBuilder AddOnDoActivity(this ITypedStateBuilder builder, string activityName, StateActionActivityInitializationBuilder parametersBuilder = null)
            => (builder as IStateBuilder).AddOnDoActivity(activityName, parametersBuilder) as ITypedStateBuilder;

        public static ITypedStateBuilder AddOnDoActivity<TActivity>(this ITypedStateBuilder builder, StateActionActivityInitializationBuilder parametersBuilder = null)
            where TActivity : Activity
            => (builder as IStateBuilder).AddOnDoActivity<TActivity>(parametersBuilder) as ITypedStateBuilder;
    }
}
