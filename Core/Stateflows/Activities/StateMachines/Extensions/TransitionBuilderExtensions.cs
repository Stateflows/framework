using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.Activities.Events;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.StateMachines.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows.Activities
{
    public static class TransitionBuilderExtensions
    {
        #region AddGuardActivity
        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddGuardActivity<TEvent>(this ITransitionBuilder<TEvent> builder, string activityName, TransitionActivityBuildAction<TEvent> buildAction = null)
            where TEvent : Event, new()
            => builder.AddGuard(
                async c =>
                {
                    var result = false;
                    if (c.TryLocateActivity(activityName, $"{c.StateMachine.Id.Instance}.{c.SourceState.Name}.{EventInfo<TEvent>.Name}.{Constants.Guard}.{c.Event.Id}", out var a))
                    {
                        var ev = StateflowsJsonConverter.Clone(c.Event);
                        await Task.Run(async () =>
                        {
                            var integratedActivityBuilder = new TransitionActivityBuilder<TEvent>(buildAction);
                            Event initializationEvent = (integratedActivityBuilder.InitializationBuilder != null)
                                ? await integratedActivityBuilder.InitializationBuilder(c)
                                : new Initialize();
                            var executionRequest = new ExecutionRequest() { InitializationEvent = initializationEvent };
                            executionRequest.AddInputToken(ev);

                            var sendResult = await a.SendCompoundAsync(
                                integratedActivityBuilder.GetSubscribe(c.StateMachine.Id),
                                new SetGlobalValues() { Values = (c.StateMachine.Values as ContextValuesCollection).Values },
                                executionRequest,
                                integratedActivityBuilder.GetUnsubscribe(c.StateMachine.Id)
                            );

                            result = (sendResult.Response.Results.Skip(2).Take(1).First()?.Response as ExecutionResponse)?.OutputTokens?.OfType<TokenHolder<bool>>()?.FirstOrDefault()?.Payload ?? false;
                        });
                    }

                    return result;
                }
            );

        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddGuardActivity<TEvent, TActivity>(this ITransitionBuilder<TEvent> builder, TransitionActivityBuildAction<TEvent> buildAction = null)
            where TEvent : Event, new()
            where TActivity : class, IActivity
            => builder.AddGuardActivity<TEvent>(Activity<TActivity>.Name, buildAction);
        #endregion

        #region AddEffectActivity
        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddEffectActivity<TEvent>(this ITransitionBuilder<TEvent> builder, string activityName, TransitionActivityBuildAction<TEvent> buildAction = null)
            where TEvent : Event, new()
            => builder.AddEffect(
                c =>
                {
                    if (c.TryLocateActivity(activityName, $"{c.StateMachine.Id.Instance}.{c.SourceState.Name}.{EventInfo<TEvent>.Name}.{Constants.Effect}.{c.Event.Id}", out var a))
                    {
                        var ev = StateflowsJsonConverter.Clone(c.Event);
                        Task.Run(async () =>
                        {
                            var integratedActivityBuilder = new TransitionActivityBuilder<TEvent>(buildAction);
                            Event initializationEvent = (integratedActivityBuilder.InitializationBuilder != null)
                                ? await integratedActivityBuilder.InitializationBuilder(c)
                                : new Initialize();
                            var executionRequest = new ExecutionRequest() { InitializationEvent = initializationEvent };
                            executionRequest.AddInputToken(ev);

                            return a.SendCompoundAsync(
                                integratedActivityBuilder.GetSubscribe(c.StateMachine.Id),
                                new SetGlobalValues() { Values = (c.StateMachine.Values as ContextValuesCollection).Values },
                                executionRequest,
                                integratedActivityBuilder.GetUnsubscribe(c.StateMachine.Id)
                            );
                        });
                    }

                    return Task.CompletedTask;
                }
            );

        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddEffectActivity<TEvent, TActivity>(this ITransitionBuilder<TEvent> builder, TransitionActivityBuildAction<TEvent> buildAction = null)
            where TEvent : Event, new()
            where TActivity : class, IActivity
            => builder.AddEffectActivity<TEvent>(Activity<TActivity>.Name, buildAction);
        #endregion
    }
}