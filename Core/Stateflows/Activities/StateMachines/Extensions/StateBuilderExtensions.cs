using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.StateMachines.Sync;
using Stateflows.Activities.Events;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class StateBuilderExtensions
    {
        internal static readonly List<string> DoActivites = new List<string>();

        public static IStateBuilder AddOnEntryActivity(this IStateBuilder builder, string activityName, StateActionActivityInitializationBuilder initializationBuilder = null, IntegratedActivityBuildAction buildAction = null)
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
                                    new ResetRequest() { Mode = ResetMode.KeepVersionAndSubscriptions },
                                    new ExecutionRequest() { InitializationEvent = initializationEvent },
                                    integratedActivityBuilder.GetUnsubscriptionRequest()
                                );
                            });
                        }
                    }
                );

        public static IStateBuilder AddOnEntryActivity<TActivity>(this IStateBuilder builder, StateActionActivityInitializationBuilder initializationBuilder = null, IntegratedActivityBuildAction buildAction = null)
            where TActivity : class, IActivity
            => AddOnEntryActivity(builder, Activity<TActivity>.Name, initializationBuilder, buildAction);

        public static IStateBuilder AddOnExitActivity(this IStateBuilder builder, string activityName, StateActionActivityInitializationBuilder initializationBuilder = null, IntegratedActivityBuildAction buildAction = null)
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
                                    new ResetRequest() { Mode = ResetMode.KeepVersionAndSubscriptions },
                                    new ExecutionRequest() { InitializationEvent = initializationEvent },
                                    integratedActivityBuilder.GetUnsubscriptionRequest()
                                );
                            });
                        }
                    }
                );

        public static IStateBuilder AddOnExitActivity<TActivity>(this IStateBuilder builder, StateActionActivityInitializationBuilder initializationBuilder = null, IntegratedActivityBuildAction buildAction = null)
            where TActivity : class, IActivity
            => AddOnExitActivity(builder, Activity<TActivity>.Name, initializationBuilder, buildAction);
    }
}
