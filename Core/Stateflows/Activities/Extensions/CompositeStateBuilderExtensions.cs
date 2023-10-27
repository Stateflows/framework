using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.Activities.Extensions;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class CompositeStateBuilderExtensions
    {
        public static ICompositeStateBuilder AddOnEntryActivity(this ICompositeStateBuilder builder, string activityName, StateActionActivityInitializationBuilder initializationBuilder = null)
            => builder
                .AddOnEntry(
                    async c =>
                    {
                        if (c.TryLocateActivity(activityName, Constants.Entry, out var a))
                        {
                            InitializationRequest initializationRequest = initializationBuilder?.Invoke(c);
                            await a.ExecuteAsync(initializationRequest);
                        }
                    }
                );

        public static ICompositeStateBuilder AddOnEntryActivity<TActivity>(this ICompositeStateBuilder builder, StateActionActivityInitializationBuilder initializationBuilder = null)
            where TActivity : Activity
            => AddOnEntryActivity(builder, ActivityInfo<TActivity>.Name, initializationBuilder);

        public static ICompositeStateBuilder AddOnExitActivity(this ICompositeStateBuilder builder, string activityName, StateActionActivityInitializationBuilder initializationBuilder = null)
            => builder
                .AddOnExit(
                    async c =>
                    {
                        if (c.TryLocateActivity(activityName, Constants.Exit, out var a))
                        {
                            InitializationRequest initializationRequest = initializationBuilder?.Invoke(c);
                            await a.ExecuteAsync(initializationRequest);
                        }
                    }
                );

        public static ICompositeStateBuilder AddOnExitActivity<TActivity>(this ICompositeStateBuilder builder, StateActionActivityInitializationBuilder initializationBuilder = null)
            where TActivity : Activity
            => AddOnExitActivity(builder, ActivityInfo<TActivity>.Name, initializationBuilder);

        public static ICompositeStateBuilder AddOnDoActivity(this ICompositeStateBuilder builder, string activityName, StateActionActivityInitializationBuilder initializationBuilder = null)
            => builder
                .AddOnEntry(
                    c =>
                    {
                        if (c.TryLocateActivity(activityName, Constants.Do, out var a))
                        {
                            InitializationRequest initializationRequest = initializationBuilder?.Invoke(c);
                            Task.Run(() => a.ExecuteAsync(initializationRequest));
                        }
                    }
                )
                .AddOnExit(
                    async c =>
                    {
                        if (c.TryLocateActivity(activityName, Constants.Do, out var a))
                        {
                            await a.Cancel();
                        }
                    }
                );

        public static ICompositeStateBuilder AddOnDoActivity<TActivity>(this ICompositeStateBuilder builder, StateActionActivityInitializationBuilder initializationBuilder = null)
            where TActivity : Activity
            => AddOnDoActivity(builder, ActivityInfo<TActivity>.Name, initializationBuilder);
    }
}
