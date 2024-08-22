using Stateflows.Activities.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class TypedCompositeStateBuilderExtensions
    {
        public static ITypedCompositeStateBuilder AddOnEntryActivity(this ITypedCompositeStateBuilder builder, string activityName, StateActionActivityBuildAction buildAction = null)
            => (builder as ICompositeStateBuilder).AddOnEntryActivity(activityName, buildAction) as ITypedCompositeStateBuilder;

        public static ITypedCompositeStateBuilder AddOnEntryActivity<TActivity>(this ITypedCompositeStateBuilder builder, StateActionActivityBuildAction buildAction = null)
            where TActivity : class, IActivity
            => (builder as ICompositeStateBuilder).AddOnEntryActivity<TActivity>(buildAction) as ITypedCompositeStateBuilder;

        public static ITypedCompositeStateBuilder AddOnExitActivity(this ITypedCompositeStateBuilder builder, string activityName, StateActionActivityBuildAction buildAction = null)
            => (builder as ICompositeStateBuilder).AddOnExitActivity(activityName, buildAction) as ITypedCompositeStateBuilder;

        public static ITypedCompositeStateBuilder AddOnExitActivity<TActivity>(this ITypedCompositeStateBuilder builder, StateActionActivityBuildAction buildAction = null)
            where TActivity : class, IActivity
            => (builder as ICompositeStateBuilder).AddOnExitActivity<TActivity>(buildAction) as ITypedCompositeStateBuilder;
    }
}
