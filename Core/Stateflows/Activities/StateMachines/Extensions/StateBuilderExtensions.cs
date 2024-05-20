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
                            InitializationRequest initializationRequest = initializationBuilder?.Invoke(c);
                            Task.Run(() =>
                            {
                                var integratedActivityBuilder = new IntegratedActivityBuilder(buildAction);

                                return a.SendCompoundAsync(
                                    integratedActivityBuilder.GetSubscriptionRequest(),
                                    new ResetRequest() { KeepVersion = true },
                                    new ExecutionRequest(initializationRequest, new List<TokenHolder>()),
                                    integratedActivityBuilder.GetUnsubscriptionRequest()
                                );
                            });
                        }
                    }
                );

        public static IStateBuilder AddOnEntryActivity<TActivity>(this IStateBuilder builder, StateActionActivityInitializationBuilder initializationBuilder = null, IntegratedActivityBuildAction buildAction = null)
            where TActivity : Activity
            => AddOnEntryActivity(builder, ActivityInfo<TActivity>.Name, initializationBuilder, buildAction);

        public static IStateBuilder AddOnExitActivity(this IStateBuilder builder, string activityName, StateActionActivityInitializationBuilder initializationBuilder = null, IntegratedActivityBuildAction buildAction = null)
            => builder
                .AddOnExit(
                    c =>
                    {
                        if (c.TryLocateActivity(activityName, Constants.Exit, out var a))
                        {
                            InitializationRequest initializationRequest = initializationBuilder?.Invoke(c);
                            Task.Run(() =>
                            {
                                var integratedActivityBuilder = new IntegratedActivityBuilder(buildAction);

                                return a.SendCompoundAsync(
                                    integratedActivityBuilder.GetSubscriptionRequest(),
                                    new ResetRequest() { KeepVersion = true },
                                    new ExecutionRequest(initializationRequest, new List<TokenHolder>()),
                                    integratedActivityBuilder.GetUnsubscriptionRequest()
                                );
                                //var request = new CompoundRequest()
                                //{
                                //    Events = new List<Event>()
                                //    {
                                //        integratedActivityBuilder.GetSubscriptionRequest(),
                                //        new ResetRequest() { KeepVersion = true },
                                //        new ExecutionRequest(initializationRequest, new List<Token>()),
                                //        integratedActivityBuilder.GetUnsubscriptionRequest(),
                                //    }
                                //};

                                //return a.RequestAsync(request);
                            });
                        }
                    }
                );

        public static IStateBuilder AddOnExitActivity<TActivity>(this IStateBuilder builder, StateActionActivityInitializationBuilder initializationBuilder = null, IntegratedActivityBuildAction buildAction = null)
            where TActivity : Activity
            => AddOnExitActivity(builder, ActivityInfo<TActivity>.Name, initializationBuilder, buildAction);
    }
}
