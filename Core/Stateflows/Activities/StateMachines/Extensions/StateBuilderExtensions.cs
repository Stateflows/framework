using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Classes;
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

        public static IStateBuilder AddOnEntryActivity(this IStateBuilder builder, string activityName, StateActionActivityBuildAction buildAction = null)
            => builder
                .AddOnEntry(
                    c =>
                    {
                        if (c.TryLocateActivity(activityName, $"{c.StateMachine.Id.Instance}.{c.CurrentState.Name}.{Constants.Entry}.{c.ExecutionTrigger.Id}", out var a))
                        {
                            Task.Run(async () =>
                            {
                                var integratedActivityBuilder = new StateActionActivityBuilder(buildAction);
                                Event initializationEvent = (integratedActivityBuilder.InitializationBuilder != null)
                                    ? await integratedActivityBuilder.InitializationBuilder(c)
                                    : new Initialize();
                                return a.SendCompoundAsync(
                                    integratedActivityBuilder.GetSubscribe(c.StateMachine.Id),
                                    new SetGlobalValues() { Values = (c.StateMachine.Values as ContextValuesCollection).Values },
                                    new ExecutionRequest() { InitializationEvent = initializationEvent },
                                    integratedActivityBuilder.GetUnsubscribe(c.StateMachine.Id)
                                );
                            });
                        }
                    }
                );

        public static IStateBuilder AddOnEntryActivity<TActivity>(this IStateBuilder builder, StateActionActivityBuildAction buildAction = null)
            where TActivity : class, IActivity
            => AddOnEntryActivity(builder, Activity<TActivity>.Name, buildAction);

        public static IStateBuilder AddOnExitActivity(this IStateBuilder builder, string activityName, StateActionActivityBuildAction buildAction = null)
            => builder
                .AddOnExit(
                    c =>
                    {
                        if (c.TryLocateActivity(activityName, $"{c.StateMachine.Id.Instance}.{c.CurrentState.Name}.{Constants.Exit}.{c.ExecutionTrigger.Id}", out var a))
                        {
                            Task.Run(async () =>
                            {
                                var integratedActivityBuilder = new StateActionActivityBuilder(buildAction);
                                Event initializationEvent = (integratedActivityBuilder.InitializationBuilder != null)
                                    ? await integratedActivityBuilder.InitializationBuilder(c)
                                    : new Initialize();
                                return a.SendCompoundAsync(
                                    integratedActivityBuilder.GetSubscribe(c.StateMachine.Id),
                                    new SetGlobalValues() { Values = (c.StateMachine.Values as ContextValuesCollection).Values },
                                    new ExecutionRequest() { InitializationEvent = initializationEvent },
                                    integratedActivityBuilder.GetUnsubscribe(c.StateMachine.Id)
                                );
                            });
                        }
                    }
                );

        public static IStateBuilder AddOnExitActivity<TActivity>(this IStateBuilder builder, StateActionActivityBuildAction buildAction = null)
            where TActivity : class, IActivity
            => AddOnExitActivity(builder, Activity<TActivity>.Name, buildAction);
    }
}
