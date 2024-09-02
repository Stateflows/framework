using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.StateMachines.Sync;
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
                                var initializationEvent = (integratedActivityBuilder.InitializationBuilder != null)
                                    ? await integratedActivityBuilder.InitializationBuilder(c)
                                    : new Initialize();

                                return a.SendCompoundAsync(ev => ev
                                    .Add(integratedActivityBuilder.GetSubscribe(c.StateMachine.Id))
                                    .Add(new SetGlobalValues() { Values = (c.StateMachine.Values as ContextValuesCollection).Values })
                                    .Add(initializationEvent)
                                    .Add(integratedActivityBuilder.GetUnsubscribe(c.StateMachine.Id))
                                );
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
                                var initializationEvent = (integratedActivityBuilder.InitializationBuilder != null)
                                    ? await integratedActivityBuilder.InitializationBuilder(c)
                                    : new Initialize();

                                return a.SendCompoundAsync(ev => ev
                                    .Add(integratedActivityBuilder.GetSubscribe(c.StateMachine.Id))
                                    .Add(new SetGlobalValues() { Values = (c.StateMachine.Values as ContextValuesCollection).Values })
                                    .Add(initializationEvent)
                                    .Add(integratedActivityBuilder.GetUnsubscribe(c.StateMachine.Id))
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
