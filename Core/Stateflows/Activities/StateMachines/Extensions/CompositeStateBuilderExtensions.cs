using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Sync;
using Stateflows.Activities.Events;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class CompositeStateBuilderExtensions
    {
        public static ICompositeStateBuilder AddOnEntryActivity(this ICompositeStateBuilder builder, string activityName, StateActionActivityInitializationBuilder initializationBuilder = null, IntegratedActivityBuildAction buildAction = null)
            => builder
                .AddOnEntry(
                    c =>
                    {
                        if (c.TryLocateActivity(activityName, Constants.Entry, out var a))
                        {
                            Event initializationEvent = initializationBuilder?.Invoke(c) ?? new Initialize();
                            Task.Run(() =>
                            {
                                var integratedActivityBuilder = new IntegratedActivityBuilder(buildAction);
                                return a.SendCompoundAsync(
                                    integratedActivityBuilder.GetSubscriptionRequest(),
                                    new Reset() { Mode = ResetMode.KeepVersionAndSubscriptions },
                                    new ExecutionRequest() { InitializationEvent = initializationEvent },
                                    integratedActivityBuilder.GetUnsubscriptionRequest()
                                );
                            });
                        }
                    }
                );

        public static ICompositeStateBuilder AddOnEntryActivity<TActivity>(this ICompositeStateBuilder builder, StateActionActivityInitializationBuilder initializationBuilder = null, IntegratedActivityBuildAction buildAction = null)
            where TActivity : class, IActivity
            => AddOnEntryActivity(builder, Activity<TActivity>.Name, initializationBuilder, buildAction);

        public static ICompositeStateBuilder AddOnExitActivity(this ICompositeStateBuilder builder, string activityName, StateActionActivityInitializationBuilder initializationBuilder = null, IntegratedActivityBuildAction buildAction = null)
            => builder
                .AddOnExit(
                    c =>
                    {
                        if (c.TryLocateActivity(activityName, Constants.Exit, out var a))
                        {
                            Event initializationEvent = initializationBuilder?.Invoke(c) ?? new Initialize();
                            Task.Run(() =>
                            {
                                var integratedActivityBuilder = new IntegratedActivityBuilder(buildAction);
                                return a.SendCompoundAsync(
                                    integratedActivityBuilder.GetSubscriptionRequest(),
                                    new Reset() { Mode = ResetMode.KeepVersionAndSubscriptions },
                                    new ExecutionRequest() { InitializationEvent = initializationEvent },
                                    integratedActivityBuilder.GetUnsubscriptionRequest()
                                );
                            });
                        }
                    }
                );

        public static ICompositeStateBuilder AddOnExitActivity<TActivity>(this ICompositeStateBuilder builder, StateActionActivityInitializationBuilder initializationBuilder = null, IntegratedActivityBuildAction buildAction = null)
            where TActivity : class, IActivity
            => AddOnExitActivity(builder, Activity<TActivity>.Name, initializationBuilder, buildAction);
    }
}
