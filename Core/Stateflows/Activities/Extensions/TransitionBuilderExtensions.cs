using System.Linq;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class TransitionBuilderExtensions
    {
        #region AddGuardActivity
        public static ITransitionBuilder<TEvent> AddGuardActivity<TEvent>(this ITransitionBuilder<TEvent> builder, string activityName, GuardActivityInitializationBuilder<TEvent> parametersBuilder = null)
            where TEvent : Event, new()
            => builder.AddGuard(
                async c => c.TryLocateActivity(activityName, Constants.Guard, out var a)
                    && ((await a.ExecuteAsync(parametersBuilder?.Invoke(c))).Response?.OutputTokens.OfType<Token<bool>>().FirstOrDefault()?.Payload ?? false)
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
                        await a.ExecuteAsync(parametersBuilder?.Invoke(c));
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