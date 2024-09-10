﻿using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Activities.Extensions;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.Activities.Events;
using Stateflows.Activities.StateMachines.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows.Activities
{
    public static class InternalTransitionBuilderExtensions
    {
        //#region AddGuardActivity
        //[DebuggerHidden]
        //public static IInternalTransitionBuilder<TEvent> AddGuardActivity<TEvent>(this IInternalTransitionBuilder<TEvent> builder, string activityName, TransitionActivityBuildAction<TEvent> buildAction = null)

        //    => builder.AddGuard(
        //        async c =>
        //        {
        //            var result = false;
        //            if (c.TryLocateActivity(activityName, $"{c.StateMachine.Id.Instance}.{c.SourceState.Name}.{typeof(TEvent).GetEventName()}.{Constants.Guard}.{c.Event.Id}", out var a))
        //            {
        //                var ev = StateflowsJsonConverter.Clone(c.Event);
        //                await Task.Run(async () =>
        //                {
        //                    var integratedActivityBuilder = new TransitionActivityBuilder<TEvent>(buildAction);
        //                    EventHolder initializationEvent = (integratedActivityBuilder.InitializationBuilder != null)
        //                        ? await integratedActivityBuilder.InitializationBuilder(c)
        //                        : new Initialize();
        //                    var executionRequest = new ExecutionRequest() { InitializationEvent = initializationEvent };
        //                    executionRequest.AddInputToken(ev);

        //                    var sendResult = await a.SendCompoundAsync(
        //                        integratedActivityBuilder.GetSubscribe(c.StateMachine.Id),
        //                        new SetGlobalValues() { Values = (c.StateMachine.Values as ContextValuesCollection).Values },
        //                        executionRequest,
        //                        integratedActivityBuilder.GetUnsubscribe(c.StateMachine.Id)
        //                    );

        //                    result = (sendResult.Response.Results.Skip(2).Take(1).First().Response as ExecutionResponse).OutputTokens.OfType<TokenHolder<bool>>().FirstOrDefault()?.Payload ?? false;
        //                });
        //            }

        //            return result;
        //        }
        //    );

        //[DebuggerHidden]
        //public static IInternalTransitionBuilder<TEvent> AddGuardActivity<TEvent, TActivity>(this IInternalTransitionBuilder<TEvent> builder, TransitionActivityBuildAction<TEvent> buildAction = null)

        //    where TActivity : class, IActivity
        //    => builder.AddGuardActivity<TEvent>(Activity<TActivity>.Name, buildAction);
        //#endregion

        //#region AddEffectActivity
        //[DebuggerHidden]
        //public static IInternalTransitionBuilder<TEvent> AddEffectActivity<TEvent>(this IInternalTransitionBuilder<TEvent> builder, string activityName, TransitionActivityBuildAction<TEvent> buildAction = null)

        //    => builder.AddEffect(
        //        c =>
        //        {
        //            if (c.TryLocateActivity(activityName, $"{c.StateMachine.Id.Instance}.{c.SourceState.Name}.{typeof(TEvent).GetEventName()}.{Constants.Effect}.{c.Event.Id}", out var a))
        //            {
        //                var ev = StateflowsJsonConverter.Clone(c.Event);
        //                Task.Run(async () =>
        //                {
        //                    var integratedActivityBuilder = new TransitionActivityBuilder<TEvent>(buildAction);
        //                    EventHolder initializationEvent = (integratedActivityBuilder.InitializationBuilder != null)
        //                        ? await integratedActivityBuilder.InitializationBuilder(c)
        //                        : new Initialize();
        //                    var executionRequest = new ExecutionRequest() { InitializationEvent = initializationEvent };
        //                    executionRequest.AddInputToken(ev);

        //                    return a.SendCompoundAsync(
        //                        integratedActivityBuilder.GetSubscribe(c.StateMachine.Id),
        //                        new SetGlobalValues() { Values = (c.StateMachine.Values as ContextValuesCollection).Values },
        //                        executionRequest,
        //                        integratedActivityBuilder.GetUnsubscribe(c.StateMachine.Id)
        //                    );
        //                });
        //            }

        //            return Task.CompletedTask;
        //        }
        //    );

        //[DebuggerHidden]
        //public static IInternalTransitionBuilder<TEvent> AddEffectActivity<TEvent, TActivity>(this IInternalTransitionBuilder<TEvent> builder, TransitionActivityBuildAction<TEvent> buildAction = null)

        //    where TActivity : class, IActivity
        //    => builder.AddEffectActivity<TEvent>(Activity<TActivity>.Name, buildAction);
        //#endregion
    }
}