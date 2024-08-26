using System.Threading.Tasks;
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
    public static class CompositeStateBuilderExtensions
    {
        public static ICompositeStateBuilder AddOnEntryActivity(this ICompositeStateBuilder builder, string activityName, StateActionActivityBuildAction buildAction = null)
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
                                return a.Compound()
                                    .AddEvent(integratedActivityBuilder.GetSubscribe(c.StateMachine.Id))
                                    .AddEvent(new SetGlobalValues() { Values = (c.StateMachine.Values as ContextValuesCollection).Values })
                                    .AddEvent(new ExecutionRequest() { InitializationEvent = initializationEvent })
                                    .AddEvent(integratedActivityBuilder.GetUnsubscribe(c.StateMachine.Id))
                                    .RequestAsync();
                            });
                        }
                    }
                );

        public static ICompositeStateBuilder AddOnEntryActivity<TActivity>(this ICompositeStateBuilder builder, StateActionActivityBuildAction buildAction = null)
            where TActivity : class, IActivity
            => AddOnEntryActivity(builder, Activity<TActivity>.Name, buildAction);

        public static ICompositeStateBuilder AddOnExitActivity(this ICompositeStateBuilder builder, string activityName, StateActionActivityBuildAction buildAction = null)
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

        public static ICompositeStateBuilder AddOnExitActivity<TActivity>(this ICompositeStateBuilder builder, StateActionActivityBuildAction buildAction = null)
            where TActivity : class, IActivity
            => AddOnExitActivity(builder, Activity<TActivity>.Name, buildAction);
    }
}
