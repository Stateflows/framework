using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Activities.Events;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.StateMachines.Interfaces;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows.Activities
{
    public static class DefaultTransitionBuilderExtensions
    {
        #region AddGuardActivity
        [DebuggerHidden]
        public static IDefaultTransitionBuilder AddGuardActivity(this IDefaultTransitionBuilder builder, string activityName, TransitionActivityBuildAction<CompletionEvent> buildAction = null)
            => builder.AddGuard(
                async c =>
                {
                    var result = false;
                    if (c.TryLocateActivity(activityName, $"{c.StateMachine.Id.Instance}.{c.SourceState.Name}.{Constants.Guard}.{c.Event.Id}", out var a))
                    {
                        var ev = StateflowsJsonConverter.Clone(c.Event);
                        await Task.Run(async () =>
                        {
                            var integratedActivityBuilder = new TransitionActivityBuilder<CompletionEvent>(buildAction);
                            Event initializationEvent = (integratedActivityBuilder.InitializationBuilder != null)
                                ? await integratedActivityBuilder.InitializationBuilder(c)
                                : new Initialize();
                            var executionRequest = new ExecutionRequest() { InitializationEvent = initializationEvent };
                            executionRequest.AddInputToken(ev);

                            var sendResult = await a.SendCompoundAsync(
                                integratedActivityBuilder.GetSubscriptionRequest(c.StateMachine.Id),
                                new SetGlobalValues() { Values = (c.StateMachine.Values as ContextValuesCollection).Values },
                                executionRequest,
                                integratedActivityBuilder.GetUnsubscriptionRequest(c.StateMachine.Id)
                            );

                            result = (sendResult.Response.Results.Skip(2).Take(1).First().Response as ExecutionResponse).OutputTokens.OfType<TokenHolder<bool>>().FirstOrDefault()?.Payload ?? false;
                        });
                    }

                    return result;
                }
            );

        [DebuggerHidden]
        public static IDefaultTransitionBuilder AddGuardActivity<TEvent, TActivity>(this IDefaultTransitionBuilder builder, TransitionActivityBuildAction<CompletionEvent> buildAction = null)
            where TActivity : class, IActivity
            => builder.AddGuardActivity(Activity<TActivity>.Name, buildAction);
        #endregion

        #region AddEffectActivity
        [DebuggerHidden]
        public static IDefaultTransitionBuilder AddEffectActivity(this IDefaultTransitionBuilder builder, string activityName, TransitionActivityBuildAction<CompletionEvent> buildAction = null)
            => builder.AddEffect(
                c =>
                {
                    if (c.TryLocateActivity(activityName, $"{c.StateMachine.Id.Instance}.{c.SourceState.Name}.{Constants.Effect}.{c.Event.Id}", out var a))
                    {
                        var ev = StateflowsJsonConverter.Clone(c.Event);
                        Task.Run(async () =>
                        {
                            var integratedActivityBuilder = new TransitionActivityBuilder<CompletionEvent>(buildAction);
                            Event initializationEvent = (integratedActivityBuilder.InitializationBuilder != null)
                                ? await integratedActivityBuilder.InitializationBuilder(c)
                                : new Initialize();
                            var executionRequest = new ExecutionRequest() { InitializationEvent = initializationEvent };
                            executionRequest.AddInputToken(ev);

                            return a.SendCompoundAsync(
                                integratedActivityBuilder.GetSubscriptionRequest(c.StateMachine.Id),
                                new SetGlobalValues() { Values = (c.StateMachine.Values as ContextValuesCollection).Values },
                                executionRequest,
                                integratedActivityBuilder.GetUnsubscriptionRequest(c.StateMachine.Id)
                            );
                        });

                        return Task.CompletedTask;
                    }
                    else
                    {
                        return Task.CompletedTask;
                    }    
                }
            );

        [DebuggerHidden]
        public static IDefaultTransitionBuilder AddEffectActivity<TActivity>(this IDefaultTransitionBuilder builder, TransitionActivityBuildAction<CompletionEvent> buildAction = null)
            where TActivity : class, IActivity
            => builder.AddEffectActivity(Activity<TActivity>.Name, buildAction);
        #endregion
    }
}