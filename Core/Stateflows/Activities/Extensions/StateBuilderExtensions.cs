using System.Threading.Tasks;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.Activities.Extensions;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public static class StateBuilderExtensions
    {
        public static IStateBuilder AddOnEntryActivity(this IStateBuilder builder, string activityName, StateActionActivityInitializationBuilder parametersBuilder = null)
            => builder
                .AddOnEntry(
                    async c =>
                    {
                        if (c.TryLocateActivity(activityName, Constants.Entry, out var a))
                        {
                            var initializationRequest = parametersBuilder?.Invoke(c) ?? new InitializationRequest();
                            await a.ExecuteAsync(initializationRequest);
                        }
                    }
                );

        public static IStateBuilder AddOnEntryActivity<TActivity>(this IStateBuilder builder, StateActionActivityInitializationBuilder parametersBuilder = null)
            where TActivity : Activity
            => AddOnEntryActivity(builder, ActivityInfo<TActivity>.Name, parametersBuilder);

        public static IStateBuilder AddOnExitActivity(this IStateBuilder builder, string activityName, StateActionActivityInitializationBuilder parametersBuilder = null)
            => builder
                .AddOnExit(
                    async c =>
                    {
                        if (c.TryLocateActivity(activityName, Constants.Entry, out var a))
                        {
                            var initializationRequest = parametersBuilder?.Invoke(c) ?? new InitializationRequest();
                            await a.ExecuteAsync(initializationRequest);
                        }
                    }
                );

        public static IStateBuilder AddOnExitActivity<TActivity>(this IStateBuilder builder, StateActionActivityInitializationBuilder parametersBuilder = null)
            where TActivity : Activity
            => AddOnExitActivity(builder, ActivityInfo<TActivity>.Name, parametersBuilder);

        public static IStateBuilder AddOnDoActivity(this IStateBuilder builder, string activityName, StateActionActivityInitializationBuilder parametersBuilder = null)
            => builder
                .AddOnEntry(
                    c =>
                    {
                        if (c.TryLocateActivity(activityName, Constants.Do, out var a))
                        {
                            var initializationRequest = parametersBuilder?.Invoke(c) ?? new InitializationRequest();
                            _ = a.ExecuteAsync(initializationRequest);
                        }
                    }
                )
                .AddOnExit(
                    async c =>
                    {
                        if (c.TryLocateActivity(activityName, Constants.Do, out var a))
                        {
                            await a.CancelAsync();
                        }
                    }
                );

        public static IStateBuilder AddOnDoActivity<TActivity>(this IStateBuilder builder, StateActionActivityInitializationBuilder parametersBuilder = null)
            where TActivity : Activity
            => AddOnDoActivity(builder, ActivityInfo<TActivity>.Name, parametersBuilder);
    }
}
