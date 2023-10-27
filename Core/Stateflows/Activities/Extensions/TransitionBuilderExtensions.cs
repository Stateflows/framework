using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.Activities.Extensions;

namespace Stateflows.Activities
{
    public static class TransitionBuilderExtensions
    {
        #region AddGuardActivity
        public static ITransitionBuilder<TEvent> AddGuardActivity<TEvent>(this ITransitionBuilder<TEvent> builder, string activityName, GuardActivityInitializationBuilder<TEvent> parametersBuilder = null)
            where TEvent : Event, new()
            => builder.AddGuard(
                c =>
                {
                    if (c.TryLocateActivity(activityName, Constants.Guard, out var a))
                    {
                        var initializationRequest = parametersBuilder?.Invoke(c) ?? new InitializationRequest();
                        return a.ExecuteAsync<bool>(initializationRequest);
                    }
                    else
                    {
                        return Task.FromResult(false);
                    }
                }
            );

        public static ITransitionBuilder<TEvent> AddGuardActivity<TEvent, TActivity>(this ITransitionBuilder<TEvent> builder, GuardActivityInitializationBuilder<TEvent> parametersBuilder = null)
            where TEvent : Event, new()
            where TActivity : Activity
            => builder.AddGuardActivity<TEvent>(ActivityInfo<TActivity>.Name, parametersBuilder);
        #endregion

        #region AddEffectActivity
        public static ITransitionBuilder<TEvent> AddEffectActivity<TEvent>(this ITransitionBuilder<TEvent> builder, string activityName, EffectActivityInitializationBuilder<TEvent> parametersBuilder = null)
            where TEvent : Event, new()
            => builder.AddEffect(
                async c =>
                {
                    if (c.TryLocateActivity(activityName, Constants.Guard, out var a))
                    {
                        var initializationRequest = parametersBuilder?.Invoke(c) ?? new InitializationRequest();
                        await a.ExecuteAsync(initializationRequest);
                    }
                }
            );

        public static ITransitionBuilder<TEvent> AddEffectActivity<TEvent, TActivity>(this ITransitionBuilder<TEvent> builder, EffectActivityInitializationBuilder<TEvent> parametersBuilder = null)
            where TEvent : Event, new()
            where TActivity : Activity
            => builder.AddEffectActivity<TEvent>(ActivityInfo<TActivity>.Name, parametersBuilder);
        #endregion
    }
}