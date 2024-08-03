using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.Activities.StateMachines.Interfaces;
using Stateflows.Activities.Events;

namespace Stateflows.Activities
{
    public static class TransitionBuilderExtensions
    {
        #region AddGuardActivity
        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddGuardActivity<TEvent>(this ITransitionBuilder<TEvent> builder, string activityName, GuardActivityInitializationBuilder<TEvent> initializationBuilder = null, IntegratedActivityBuildAction buildAction = null)
            where TEvent : Event, new()
            => builder.AddGuard(
                    async c =>
                    {
                        var result = false;
                        if (c.TryLocateActivity(activityName, Constants.Entry, out var a))
                        {
                            Event initializationEvent = initializationBuilder?.Invoke(c) ?? new Initialize();
                            await Task.Run(async () =>
                            {
                                var integratedActivityBuilder = new IntegratedActivityBuilder(buildAction);
                                var sendResult = await a.SendCompoundAsync(
                                    integratedActivityBuilder.GetSubscriptionRequest(),
                                    new ResetRequest() { Mode = ResetMode.KeepVersionAndSubscriptions },
                                    new ExecutionRequest() { InitializationEvent = initializationEvent },
                                    integratedActivityBuilder.GetUnsubscriptionRequest()
                                );

                                result = (sendResult.Response.Results.Skip(2).Take(1).First().Response as ExecutionResponse).OutputTokens.OfType<TokenHolder<bool>>().FirstOrDefault()?.Payload ?? false;
                            });
                        }

                        return result;
                    }
            );

        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddGuardActivity<TEvent, TActivity>(this ITransitionBuilder<TEvent> builder, GuardActivityInitializationBuilder<TEvent> initializationBuilder = null, IntegratedActivityBuildAction buildAction = null)
            where TEvent : Event, new()
            where TActivity : class, IActivity
            => builder.AddGuardActivity<TEvent>(Activity<TActivity>.Name, initializationBuilder);
        #endregion

        #region AddEffectActivity
        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddEffectActivity<TEvent>(this ITransitionBuilder<TEvent> builder, string activityName, EffectActivityInitializationBuilder<TEvent> initializationBuilder = null, IntegratedActivityBuildAction buildAction = null)
            where TEvent : Event, new()
            => builder.AddEffect(
                c =>
                {
                    if (c.TryLocateActivity(activityName, Constants.Entry, out var a))
                    {
                        Event initializationEvent = initializationBuilder?.Invoke(c) ?? new Initialize();
                        return Task.Run(() =>
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
                    else
                    {
                        return Task.CompletedTask;
                    }    
                }
            );

        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddEffectActivity<TEvent, TActivity>(this ITransitionBuilder<TEvent> builder, EffectActivityInitializationBuilder<TEvent> initializationBuilder = null, IntegratedActivityBuildAction buildAction = null)
            where TEvent : Event, new()
            where TActivity : class, IActivity
            => builder.AddEffectActivity<TEvent>(Activity<TActivity>.Name, initializationBuilder);
        #endregion
    }
}